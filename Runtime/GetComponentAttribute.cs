#region Header
/* ============================================ 
 *			    Strix Unity Library
 *		https://github.com/KorStrix/Unity_GetComponentAttribute
 *	============================================ 	
 *	작성자 : Strix
 *	
 *	기능 : 
 *	오리지널 소스코드의 경우 에디터 - Inspector에서 봐야만 갱신이 되었는데,
 *	현재는 SCManagerGetComponent.DoUpdateGetComponentAttribute 를 호출하면 갱신하도록 변경하였습니다.
 *	Awake에서 호출하시면 됩니다.
 *	
 *	Private 변수는 갱신되지 않습니다.
 *	
 *	오리지널 소스 링크
 *	https://openlevel.postype.com/post/683269
   ============================================ */
#endregion Header

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

public interface IGetComponentAttribute
{
    object GetComponent(MonoBehaviour pMono, Type pElementType);
    bool bIsPrint_OnNotFound_GetComponent { get; }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public abstract class GetComponentAttributeBase : PropertyAttribute, IGetComponentAttribute
{
    public bool bIsPrint_OnNotFound_GetComponent => bIsPrint_OnNotFound;
    public bool bIsPrint_OnNotFound;

    public abstract object GetComponent(MonoBehaviour pMono, Type pElementType);
}

public class GetComponentAttribute : GetComponentAttributeBase
{
    public override object GetComponent(MonoBehaviour pMono, Type pElementType)
    {
        return GetComponentAttributeSetter.Event_GetComponent(pMono, pElementType);
    }
}

public class GetComponentInParentAttribute : GetComponentAttributeBase
{
    public override object GetComponent(MonoBehaviour pMono, Type pElementType)
    {
        return GetComponentAttributeSetter.Event_GetComponentInParents(pMono, pElementType);
    }
}



public interface IGetComponentChildrenAttribute : IGetComponentAttribute
{
    bool bSearch_By_ComponentName_ForGetComponent { get; }
    string strComponentName_ForGetComponent { get; }
}

public class GetComponentInChildrenAttribute : GetComponentAttributeBase, IGetComponentChildrenAttribute
{
    public bool bSearch_By_ComponentName_ForGetComponent => bSearch_By_ComponentName;
    public string strComponentName_ForGetComponent => strComponentName;


    public bool bSearch_By_ComponentName;
    public bool bInclude_OnDisable;
    public string strComponentName;

    public GetComponentInChildrenAttribute(bool bInclude_OnDisable = true)
    {
        bSearch_By_ComponentName = false;
        this.bInclude_OnDisable = bInclude_OnDisable;
    }

    public GetComponentInChildrenAttribute(bool bInclude_OnDisable, bool bIsPrint_OnNotFound = true)
    {
        this.bInclude_OnDisable = bInclude_OnDisable;
        bSearch_By_ComponentName = false;
        this.bIsPrint_OnNotFound = bIsPrint_OnNotFound;
    }

    public GetComponentInChildrenAttribute(Object pComponentName)
    {
        bInclude_OnDisable = true;
        strComponentName = pComponentName.ToString();
        bSearch_By_ComponentName = true;
    }

    public GetComponentInChildrenAttribute(Object pComponentName, bool bInclude_OnDisable)
    {
        strComponentName = pComponentName.ToString();
        bSearch_By_ComponentName = true;
        this.bInclude_OnDisable = bInclude_OnDisable;
    }

    public GetComponentInChildrenAttribute(Object pComponentName, bool bInclude_OnDisable, bool bIsPrint_OnNotFound = true)
    {
        this.bInclude_OnDisable = bInclude_OnDisable;

        strComponentName = pComponentName.ToString();
        bSearch_By_ComponentName = true;
        this.bIsPrint_OnNotFound = bIsPrint_OnNotFound;
    }

    public override object GetComponent(MonoBehaviour pMono, Type pElementType)
    {
        return GetComponentAttributeSetter.Event_GetComponentInChildren(pMono, pElementType, bInclude_OnDisable, bSearch_By_ComponentName, strComponentName);
    }
}


/// <summary>
/// GetComponent Attribute가 적힌 필드 / 프로퍼티를 할당시켜주는 Static Class
/// </summary>
public static class GetComponentAttributeSetter
{
    public static UnityEngine.Object[] ExtractSameNameArray(string strObjectName, UnityEngine.Object[] arrComponentFind)
    {
        if (arrComponentFind == null)
            return new UnityEngine.Object[0];

        return arrComponentFind.Where(p => p.name.Equals(strObjectName)).ToArray();
    }

    public static void DoUpdate_GetComponentAttribute(MonoBehaviour pMono) =>  DoUpdate_GetComponentAttribute(pMono, pMono);

    public static void DoUpdate_GetComponentAttribute(MonoBehaviour pMono, object pClass_Anything)
    {
        // BindingFlags를 일일이 써야 잘 동작한다..
        Type pType = pClass_Anything.GetType();
        List<MemberInfo> listMembers = new List<MemberInfo>(pType.GetFields(BindingFlags.Public | BindingFlags.Instance));
        listMembers.AddRange(pType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
        listMembers.AddRange(pType.GetProperties(BindingFlags.Public | BindingFlags.Instance));
        listMembers.AddRange(pType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance));

        var arrMembers_Filtered = listMembers.Where(p => p.GetCustomAttributes().Any());
        foreach(var pMember in arrMembers_Filtered)
            DoUpdate_GetComponentAttribute(pMono, pClass_Anything, pMember);
    }

    public static void DoUpdate_GetComponentAttribute(MonoBehaviour pTargetMono, object pMemberOwner, MemberInfo pMemberInfo)
    {
        if (pMemberInfo == null)
            return;


        Type pMemberType = pMemberInfo.MemberType();
        IGetComponentAttribute[] arrCustomAttributes = pMemberInfo.GetCustomAttributes(true).OfType<IGetComponentAttribute>().ToArray();
        foreach(var pGetComponentAttribute in arrCustomAttributes)
        {
            object pComponent = SetMember_FromGetComponent(pTargetMono, pMemberOwner, pMemberInfo, pMemberType, pGetComponentAttribute);
            if (pComponent != null)
                continue;

            if (pGetComponentAttribute.bIsPrint_OnNotFound_GetComponent)
            {
                if (pGetComponentAttribute is GetComponentInChildrenAttribute pAttribute && pAttribute.bSearch_By_ComponentName)
                    Debug.LogError(pTargetMono.name + string.Format(".{0}<{1}>({2}) Result == null", pGetComponentAttribute.GetType().Name, pMemberType, pAttribute.strComponentName), pTargetMono);
                else
                    Debug.LogError(pTargetMono.name + string.Format(".{0}<{1}> Result == null", pGetComponentAttribute.GetType().Name, pMemberType), pTargetMono);
            }
        }
    }

    // ====================================================================================================================


    public static object Event_GetComponent(MonoBehaviour pMono, Type pElementType)
    {
        // ReSharper disable once PossibleNullReferenceException
        MethodInfo getter = typeof(MonoBehaviour)
                 .GetMethod("GetComponents", new Type[0])
                 .MakeGenericMethod(pElementType);

        return getter.Invoke(pMono, null);
    }

    public static object Event_GetComponentInChildren(MonoBehaviour pMono, Type pElementType, bool bInclude_DeActive, bool bSearch_By_ComponentName, string strComponentName)
    {
        object pObjectReturn;

        MethodInfo pGetMethod = typeof(MonoBehaviour).GetMethod("GetComponentsInChildren", new[] { typeof(bool) }).MakeGenericMethod(pElementType);

        if (pElementType.HasElementType)
		    pElementType = pElementType.GetElementType();

        if(pElementType == typeof(GameObject))
        {
	        pElementType = typeof(Transform);
	        pObjectReturn = Convert_TransformArray_To_GameObjectArray(pGetMethod.Invoke(pMono, new object[] { bInclude_DeActive }))
        }
        else
        {
	        pObjectReturn = pGetMethod.Invoke(pMono, new object[] { bInclude_DeActive });
        }

        if (bSearch_By_ComponentName)
            return ExtractSameNameArray(strComponentName, pObjectReturn as UnityEngine.Object[]);
        return pObjectReturn;
    }

    public static object Event_GetComponentInParents(MonoBehaviour pTargetMono, Type pElementType)
    {
        bool bTypeIsGameObject = pElementType == typeof(GameObject);
        if (bTypeIsGameObject)
            pElementType = typeof(Transform);

        // ReSharper disable once PossibleNullReferenceException
        MethodInfo pGetMethod = typeof(MonoBehaviour).
            GetMethod("GetComponentsInParent", new Type[] { }).
            MakeGenericMethod(pElementType);

        if (bTypeIsGameObject)
            return Convert_TransformArray_To_GameObjectArray(pGetMethod.Invoke(pTargetMono, new object[] { }));
        return pGetMethod.Invoke(pTargetMono, new object[] { });
    }

    // ====================================================================================================================

    private static object SetMember_FromGetComponent(MonoBehaviour pMono, object pMemberOwner, MemberInfo pMemberInfo, Type pMemberType, IGetComponentAttribute iGetComponentAttribute)
    {
        var pComponent = GetComponent(pMono, pMemberType, iGetComponentAttribute);
        if (pComponent == null)
            return null;

        if (pMemberType.IsGenericType)
        {
            pMemberInfo.SetValue_Extension(pMemberOwner, pComponent);
        }
        else
        {
            if (pMemberType.HasElementType == false)
            {
                if (pComponent is Array arrComponent)
                    pMemberInfo.SetValue_Extension(pMemberOwner, arrComponent.Length != 0 ? arrComponent.GetValue(0) : null);
            }
            else
            {
                if (pComponent is Array arrComponent)
                {
                    if (pMemberType.GetElementType() == typeof(GameObject))
                    {
                        pMemberInfo.SetValue_Extension(pMemberOwner, arrComponent.Cast<GameObject>().ToArray());
                    }
                    else
                    {
                        // Object[]를 Type[]로 NoneGeneric하게 바꿔야함..;
                        Array ConvertedArray = Array.CreateInstance(pMemberType.GetElementType(), arrComponent.Length);
                        Array.Copy(arrComponent, ConvertedArray, arrComponent.Length);

                        pMemberInfo.SetValue_Extension(pMemberOwner, ConvertedArray);
                    }
                }
                else
                {
                    if (pMemberType == typeof(GameObject))
                        pMemberInfo.SetValue_Extension(pMemberOwner, ((Component)pComponent).gameObject);
                    else
                        pMemberInfo.SetValue_Extension(pMemberOwner, pComponent);
                }
            }
        }

        return pComponent;
    }

    private static object GetComponent(MonoBehaviour pMono, Type pMemberType, IGetComponentAttribute iGetComponentAttribute)
    {
        return pMemberType.IsGenericType ?
            GetComponent_OnGeneric(iGetComponentAttribute, pMono, pMemberType) : 
            iGetComponentAttribute.GetComponent(pMono, pMemberType.HasElementType ? pMemberType.GetElementType() : pMemberType);
    }

    private static object GetComponent_OnGeneric(IGetComponentAttribute iGetComponentAttribute, MonoBehaviour pMono, Type pTypeField)
    {
        Type pTypeField_Generic = pTypeField.GetGenericTypeDefinition();
        Type[] arrArgumentsType = pTypeField.GetGenericArguments();

        object pComponent = null;
        if (pTypeField_Generic == typeof(List<>))
            pComponent = GetComponent_OnList(iGetComponentAttribute, pMono, pTypeField, arrArgumentsType[0]);
        else if (pTypeField_Generic == typeof(Dictionary<,>))
            pComponent = GetComponent_OnDictionary(iGetComponentAttribute as IGetComponentChildrenAttribute, pMono, pTypeField, arrArgumentsType[0], arrArgumentsType[1]);

        return pComponent;
    }

    private static object GetComponent_OnList(IGetComponentAttribute iGetComponentAttribute, MonoBehaviour pMono, Type pTypeMember, Type pElementType)
    {
        Array arrComponent = iGetComponentAttribute.GetComponent(pMono, pElementType) as Array;
        if (arrComponent == null || arrComponent.Length == 0)
            return null;

        return Create_GenericList(pTypeMember, arrComponent);
    }

    private static object Create_GenericList(Type pTypeMember, IEnumerable arrComponent)
    {
        var pInstanceList = Activator.CreateInstance(pTypeMember);
        if (arrComponent == null)
            return pInstanceList;

        var Method_Add = pTypeMember.GetMethod("Add");
        var pIter = arrComponent.GetEnumerator();

        // ReSharper disable once PossibleNullReferenceException
        while (pIter.MoveNext())
            Method_Add.Invoke(pInstanceList, new[] { pIter.Current });

        return pInstanceList;
    }

    private static object GetComponent_OnDictionary(IGetComponentChildrenAttribute pAttributeInChildren, MonoBehaviour pMono, Type pMemberType, Type pType_DictionaryKey, Type pType_DictionaryValue)
    {
        if(pAttributeInChildren == null)
        {
            Debug.LogError($"Dictionary Field Type Not Support Non-IGetComponentChildrenAttribute - {pType_DictionaryKey.Name}");
            return null;
        }

        if (pType_DictionaryKey != typeof(string) && pType_DictionaryKey.IsEnum == false)
        {
            Debug.LogError($"Not Support Dictionary Key - {pType_DictionaryKey.Name} - pType_DictionaryKey != typeof(string) && pType_DictionaryKey.IsEnum == false");
            return null;
        }

        object pComponent = null;
        bool bValue_Is_Collection = pType_DictionaryValue.IsGenericType || pType_DictionaryValue.HasElementType;
        Type pTypeChild_OnValueIsCollection = null;
        if (bValue_Is_Collection)
        {
            // Dictionary의 Value타입은 항상 단일인자라는 가정하로 구현
            pTypeChild_OnValueIsCollection = pType_DictionaryValue.IsGenericType ? pType_DictionaryValue.GenericTypeArguments[0] : pType_DictionaryValue.GetElementType();
            pComponent = GetComponent(pMono, pType_DictionaryValue, pAttributeInChildren);
        }
        else
            pComponent = pAttributeInChildren.GetComponent(pMono, pType_DictionaryValue);

        IEnumerable arrChildrenComponent = pComponent as IEnumerable;
        if (arrChildrenComponent == null)
            return null;


        MethodInfo Method_Add = pMemberType.GetMethod("Add", new[] {
                                pType_DictionaryKey, pType_DictionaryValue });

        Object pInstanceDictionary = Activator.CreateInstance(pMemberType);
        if (bValue_Is_Collection)
        {
            if (pType_DictionaryKey == typeof(string))
            {
                arrChildrenComponent.OfType<UnityEngine.Object>().
                    GroupBy(p => p.name).
                    ToList().
                    ForEach(pGroup => 
                    AddDictionary_OnValueIsCollection(pMono, pType_DictionaryValue, pGroup, pTypeChild_OnValueIsCollection, Method_Add, pInstanceDictionary, (key) => key));
            }
            else if (pType_DictionaryKey.IsEnum)
            {
                HashSet<string> setEnumName = new HashSet<string>(Enum.GetNames(pType_DictionaryKey));

                arrChildrenComponent.OfType<UnityEngine.Object>().
                    GroupBy(p => p.name).
                    Where(p => setEnumName.Contains(p.Key)).
                    ToList().
                    ForEach(pGroup =>
                    AddDictionary_OnValueIsCollection(pMono, pType_DictionaryValue, pGroup, pTypeChild_OnValueIsCollection, Method_Add, pInstanceDictionary, (key) => Enum.Parse(pType_DictionaryKey, key, true)));
            }
        }
        else
        {
            if (pType_DictionaryKey == typeof(string))
            {
                arrChildrenComponent.OfType<UnityEngine.Object>().
                    ToList().
                    ForEach(pUnityObject => AddDictionary(pMono, Method_Add, pInstanceDictionary, pUnityObject, (key) => key));
            }
            else if (pType_DictionaryKey.IsEnum)
            {
                HashSet<string> setEnumName = new HashSet<string>(Enum.GetNames(pType_DictionaryKey));

                arrChildrenComponent.OfType<UnityEngine.Object>().
                    Where(p => setEnumName.Contains(p.name)).
                    ToList().
                    ForEach(pUnityObject => AddDictionary(pMono, Method_Add, pInstanceDictionary, pUnityObject, (key) => Enum.Parse(pType_DictionaryKey, pUnityObject.name, true)));
            }
        }

        return pInstanceDictionary;
    }

    private static void AddDictionary(MonoBehaviour pMono, MethodInfo Method_Add, object pInstanceDictionary, UnityEngine.Object pUnityObject, Func<string, object> OnSelectDictionaryKey)
    {
        try
        {
            Method_Add.Invoke(pInstanceDictionary, new [] { OnSelectDictionaryKey(pUnityObject.name), pUnityObject});
        }
        catch (Exception e)
        {
            Debug.LogError(pUnityObject.name + " GetComponent - GetComponent_OnDictionary - Overlap Key MonoType : " + pMono.GetType() + e, pMono);
        }
    }

    private static void AddDictionary_OnValueIsCollection(MonoBehaviour pMono, Type pType_DictionaryValue, IGrouping<string, UnityEngine.Object> pGroup, Type pTypeChild_OnValueIsCollection, MethodInfo Method_Add, object pInstanceDictionary, Func<string, object> OnSelectDictionaryKey)
    {
        try
        {
            var arrChildrenObject = pGroup.ToArray();
            if (pType_DictionaryValue.IsArray)
            {
                Array ConvertedArray = Array.CreateInstance(pTypeChild_OnValueIsCollection, arrChildrenObject.Length);
                Array.Copy(arrChildrenObject, ConvertedArray, arrChildrenObject.Length);
                Method_Add.Invoke(pInstanceDictionary, new [] { OnSelectDictionaryKey(pGroup.Key), ConvertedArray});
            }
            else
            {
                if (pType_DictionaryValue.IsGenericType)
                    Method_Add.Invoke(pInstanceDictionary, new [] { OnSelectDictionaryKey(pGroup.Key), Create_GenericList(pType_DictionaryValue, arrChildrenObject)});
                else
                    Method_Add.Invoke(pInstanceDictionary, new [] { OnSelectDictionaryKey(pGroup.Key), arrChildrenObject});
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e, pMono);
        }
    }

    private static GameObject[] Convert_TransformArray_To_GameObjectArray(object pObject)
    {
        Transform[] arrTransform = pObject as Transform[];
        if (arrTransform == null)
            return new GameObject[0];

        return arrTransform.Select(p => p.gameObject).ToArray();
    }
}



#region ExtensionMethod

public static class Component_Extension
{
    public static Component GetComponentInChildren_SameName(this Component pTarget, string strObjectName, Type pComponentType, bool bInclude_OnDisable) => GetComponentsInChildrenArray_SameName(pTarget, strObjectName, pComponentType, bInclude_OnDisable).FirstOrDefault();

    public static Component[] GetComponentsInChildrenArray_SameName(this Component pTarget, string strObjectName, Type pComponentType, bool bInclude_OnDisable)
    {
        Component[] arrComponentFind = null;
        if (pComponentType == typeof(GameObject))
            arrComponentFind = pTarget.transform.GetComponentsInChildren(typeof(Transform), true);
        else
            arrComponentFind = pTarget.transform.GetComponentsInChildren(pComponentType, bInclude_OnDisable);

        return ExtractSameNameArray(strObjectName, arrComponentFind);
    }

    public static Component[] ExtractSameNameArray(string strObjectName, Component[] arrComponentFind)
    {
        if (arrComponentFind == null)
            return new Component[0];

        return arrComponentFind.Where(p => p.name.Equals(strObjectName)).ToArray();
    }
}

public static class MemberInfo_Extension
{
    public static Type MemberType(this MemberInfo pMemberInfo)
    {
        FieldInfo pFieldInfo = pMemberInfo as FieldInfo;
        if (pFieldInfo != null)
            return pFieldInfo.FieldType;

        PropertyInfo pProperty = pMemberInfo as PropertyInfo;
        if (pProperty != null)
            return pProperty.PropertyType;

        return null;
    }

    public static void SetValue_Extension(this MemberInfo pMemberInfo, object pTarget, object pValue)
    {
        FieldInfo pFieldInfo = pMemberInfo as FieldInfo;
        if (pFieldInfo != null)
            pFieldInfo.SetValue(pTarget, pValue);

        PropertyInfo pProperty = pMemberInfo as PropertyInfo;
        if (pProperty != null)
            pProperty.SetValue(pTarget, pValue, null);
    }
}

#endregion
