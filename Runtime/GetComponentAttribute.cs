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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

public interface IGetComponentAttribute
{
    object GetComponent(MonoBehaviour pTargetMono, Type pElementType);
    bool bIsPrint_OnNotFound_GetComponent { get; }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public abstract class GetComponentAttributeBase : PropertyAttribute, IGetComponentAttribute
{
    public bool bIsPrint_OnNotFound_GetComponent => bIsPrint_OnNotFound;
    public bool bIsPrint_OnNotFound;

    public abstract object GetComponent(MonoBehaviour pTargetMono, Type pElementType);
}

public class GetComponentAttribute : GetComponentAttributeBase
{
    public override object GetComponent(MonoBehaviour pTargetMono, Type pElementType)
    {
        return GetComponentAttributeHelper.Event_GetComponent(pTargetMono, pElementType);
    }
}

public class GetComponentInChildrenAttribute : GetComponentAttributeBase
{
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

    public override object GetComponent(MonoBehaviour pTargetMono, Type pElementType)
    {
        return GetComponentAttributeHelper.Event_GetComponentInChildren(pTargetMono, pElementType, bInclude_OnDisable, bSearch_By_ComponentName, strComponentName);
    }
}

public class GetComponentInParentAttribute : GetComponentAttributeBase
{
    public override object GetComponent(MonoBehaviour pTargetMono, Type pElementType)
    {
        return GetComponentAttributeHelper.Event_GetComponentInParents(pTargetMono, pElementType);
    }
}


public static class GetComponentAttributeHelper
{
    public static List<UnityEngine.Object> ExtractSameNameList(string strObjectName, UnityEngine.Object[] arrComponentFind)
    {
        List<UnityEngine.Object> listReturn = new List<UnityEngine.Object>();
        if (arrComponentFind != null)
        {
            for (int i = 0; i < arrComponentFind.Length; i++)
            {
                if (arrComponentFind[i].name.Equals(strObjectName))
                    listReturn.Add(arrComponentFind[i]);
            }
        }

        return listReturn;
    }

    public static void DoUpdate_GetComponentAttribute(MonoBehaviour pTarget)
    {
        DoUpdate_GetComponentAttribute(pTarget, pTarget);
    }

    public static void DoUpdate_GetComponentAttribute(MonoBehaviour pMonobehaviourOwner, object pClass_Anything)
    {
        // BindingFlags를 일일이 써야 잘 동작한다..
        Type pType = pClass_Anything.GetType();
        List<MemberInfo> listMembers = new List<MemberInfo>(pType.GetFields(BindingFlags.Public | BindingFlags.Instance));
        listMembers.AddRange(pType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
        listMembers.AddRange(pType.GetProperties(BindingFlags.Public | BindingFlags.Instance));
        listMembers.AddRange(pType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance));

        var arrMembers_Filtered = listMembers.Where(p => p.GetCustomAttributes().Any());
        foreach(var pMember in arrMembers_Filtered)
            DoUpdate_GetComponentAttribute(pMonobehaviourOwner, pClass_Anything, pMember);
    }

    public static void DoUpdate_GetComponentAttribute(MonoBehaviour pTargetMono, object pMemberOwner, MemberInfo pMemberInfo)
    {
        object[] arrCustomAttributes = pMemberInfo.GetCustomAttributes(true);
        for (int i = 0; i < arrCustomAttributes.Length; i++)
        {
            IGetComponentAttribute pGetComponentAttribute = arrCustomAttributes[i] as IGetComponentAttribute;
            if (pGetComponentAttribute == null)
                continue;

            Type pTypeMember = pMemberInfo.MemberType();
            
            object pComponent = null;
            if (pTypeMember.IsGenericType)
                pComponent = SetMember_OnGeneric(pGetComponentAttribute, pTargetMono, pMemberOwner, pMemberInfo, pTypeMember);
            else if (pTypeMember.HasElementType)
                pComponent = pGetComponentAttribute.GetComponent(pTargetMono, pTypeMember.GetElementType());
            else
                pComponent = pGetComponentAttribute.GetComponent(pTargetMono, pTypeMember);

            if (pComponent == null)
            {
                if (pGetComponentAttribute.bIsPrint_OnNotFound_GetComponent)
                {
                    if (pGetComponentAttribute is GetComponentInChildrenAttribute pAttribute && pAttribute.bSearch_By_ComponentName)
                        Debug.LogError(pTargetMono.name + string.Format(".{0}<{1}>({2}) Result == null", pGetComponentAttribute.GetType().Name, pTypeMember, pAttribute.strComponentName), pTargetMono);
                    else
                        Debug.LogError(pTargetMono.name + string.Format(".{0}<{1}> Result == null", pGetComponentAttribute.GetType().Name, pTypeMember), pTargetMono);
                }

                continue;
            }

            if (pTypeMember.IsGenericType == false)
            {
                if (pTypeMember.HasElementType == false)
                {
                    if (pComponent is Array arrComponent && arrComponent.Length != 0)
                        pMemberInfo.SetValue_Extension(pMemberOwner, arrComponent.GetValue(0));
                }
                else
                {
                    if (pComponent is Array arrComponent && arrComponent.Length != 0)
                    {
                        if (pTypeMember.GetElementType() == typeof(GameObject))
                            pMemberInfo.SetValue_Extension(pMemberOwner, arrComponent.Cast<GameObject>().ToArray());
                        else
                        {
                            // Object[]를 Type[]로 바꿔야함..;
                            Array ConvertedArray = Array.CreateInstance(pTypeMember.GetElementType(), arrComponent.Length);
                            Array.Copy(arrComponent, ConvertedArray, arrComponent.Length);

                            pMemberInfo.SetValue_Extension(pMemberOwner, ConvertedArray);
                        }
                    }
                    else
                    {
                        if (pTypeMember == typeof(GameObject))
                            pMemberInfo.SetValue_Extension(pMemberOwner, ((Component)pComponent).gameObject);
                        else
                            pMemberInfo.SetValue_Extension(pMemberOwner, pComponent);
                    }
                }
            }
        }
    }

    // ====================================================================================================================

    public static object Event_GetComponent(MonoBehaviour pTargetMono, Type pElementType)
    {
        MethodInfo getter = typeof(MonoBehaviour)
                 .GetMethod("GetComponents", new Type[0])
                 .MakeGenericMethod(pElementType);

        return getter.Invoke(pTargetMono, null);
    }

    public static object Event_GetComponentInChildren(MonoBehaviour pTargetMono, Type pElementType, bool bInclude_DeActive, bool bSearch_By_ComponentName, string strComponentName)
    {
        object pObjectReturn;

        if (pElementType.HasElementType)
            pElementType = pElementType.GetElementType();

        bool bTypeIsGameObject = pElementType == typeof(GameObject);
        if (bTypeIsGameObject)
            pElementType = typeof(Transform);

        MethodInfo pGetMethod = typeof(MonoBehaviour).
            GetMethod("GetComponentsInChildren", new[] { typeof(bool) }).
            MakeGenericMethod(pElementType);

        if (bTypeIsGameObject)
            pObjectReturn = Convert_TransformArray_To_GameObjectArray(pTargetMono, pGetMethod.Invoke(pTargetMono, new object[] { bInclude_DeActive }));
        else
            pObjectReturn = pGetMethod.Invoke(pTargetMono, new object[] { bInclude_DeActive });

        if (bSearch_By_ComponentName)
            return ExtractSameNameList(strComponentName, pObjectReturn as UnityEngine.Object[]).ToArray();
        return pObjectReturn;
    }

    public static object Event_GetComponentInParents(MonoBehaviour pTargetMono, Type pElementType)
    {
        bool bTypeIsGameObject = pElementType == typeof(GameObject);
        if (bTypeIsGameObject)
            pElementType = typeof(Transform);

        MethodInfo pGetMethod = typeof(MonoBehaviour).
            GetMethod("GetComponentsInParent", new Type[] { }).
            MakeGenericMethod(pElementType);

        if (bTypeIsGameObject)
            return Convert_TransformArray_To_GameObjectArray(pTargetMono, pGetMethod.Invoke(pTargetMono, new object[] { }));
        return pGetMethod.Invoke(pTargetMono, new object[] { });
    }

    // ====================================================================================================================

    private static object SetMember_OnGeneric(IGetComponentAttribute pGetComponentAttribute, MonoBehaviour pTargetMono, object pMemberOwner, MemberInfo pMember, Type pTypeField)
    {
        object pComponent = null;
        Type pTypeField_Generic = pTypeField.GetGenericTypeDefinition();
        Type[] arrArgumentsType = pTypeField.GetGenericArguments();

        if (pTypeField_Generic == typeof(List<>))
            pComponent = SetMember_OnList(pGetComponentAttribute, pTargetMono, pMemberOwner, pMember, pTypeField, arrArgumentsType);
        else if (pTypeField_Generic == typeof(Dictionary<,>))
            pComponent = SetMember_OnDictionary(pGetComponentAttribute, pTargetMono, pMemberOwner, pMember, pTypeField, arrArgumentsType[0], arrArgumentsType[1]);

        return pComponent;
    }

    private static object SetMember_OnList(IGetComponentAttribute pGetComponentAttribute, MonoBehaviour pTargetMono, object pMemberOwner, MemberInfo pMember, Type pTypeField, Type[] arrArgumentsType)
    {
        object pComponent = pGetComponentAttribute.GetComponent(pTargetMono, arrArgumentsType[0]);
        Array arrComponent = pComponent as Array;
        var Method_Add = pTypeField.GetMethod("Add");
        var pInstanceList = Activator.CreateInstance(pTypeField);

        for (int i = 0; i < arrComponent.Length; i++)
            Method_Add.Invoke(pInstanceList, new[] { arrComponent.GetValue(i) });

        pMember.SetValue_Extension(pMemberOwner, pInstanceList);
        return pComponent;
    }

    private static object SetMember_OnDictionary(IGetComponentAttribute pAttributeInChildren, MonoBehaviour pTargetMono, object pMemberOwner, MemberInfo pMember, Type pTypeField, Type pType_DictionaryKey, Type pType_DictionaryValue)
    {
        object pComponent = pAttributeInChildren.GetComponent(pTargetMono, pType_DictionaryValue);
        Array arrComponent = pComponent as Array;

        if (arrComponent == null || arrComponent.Length == 0)
        {
            return null;
        }

        var Method_Add = pTypeField.GetMethod("Add", new[] {
                                pType_DictionaryKey, pType_DictionaryValue });

        var pInstanceDictionary = Activator.CreateInstance(pTypeField);
        if (pType_DictionaryKey == typeof(string))
        {
            for (int i = 0; i < arrComponent.Length; i++)
            {
                UnityEngine.Object pComponentChild = arrComponent.GetValue(i) as UnityEngine.Object;

                try
                {
                    Method_Add.Invoke(pInstanceDictionary, new object[] {
                                pComponentChild.name,
                                pComponentChild });
                }
                catch
                {
                    Debug.LogError(pComponentChild.name + " Get Component - Dictionary Add - Overlap Key MonoType : " + pTargetMono.GetType() + "/Member : " + pMember.Name, pTargetMono);
                }
            }
        }
        else if (pType_DictionaryKey.IsEnum)
        {
            HashSet<string> setEnumName = new HashSet<string>(Enum.GetNames(pType_DictionaryKey));
            for (int i = 0; i < arrComponent.Length; i++)
            { 
                try
                {
                    UnityEngine.Object pComponentChild = arrComponent.GetValue(i) as UnityEngine.Object;
                    if (setEnumName.Contains(pComponentChild.name) == false)
                        continue;

                    var pEnum = Enum.Parse(pType_DictionaryKey, pComponentChild.name, true);
                    Method_Add.Invoke(pInstanceDictionary, new[] {
                                    pEnum,
                                    pComponentChild });
                }
                catch
                {
                    // ignored
                }
            }
        }

        pMember.SetValue_Extension(pMemberOwner, pInstanceDictionary);
        return pComponent;
    }

    private static GameObject[] Convert_TransformArray_To_GameObjectArray(MonoBehaviour pTargetMono, object pObject)
    {
        object[] arrObject = pObject as object[];
        GameObject[] arrObjectReturn = new GameObject[arrObject.Length];
        for (int i = 0; i < arrObject.Length; i++)
            arrObjectReturn[i] = (arrObject[i] as Transform).gameObject;
        return arrObjectReturn;
    }
}



#region ExtensionMethod

public static class Component_Extension
{
    public static UnityEngine.Object GetComponentInChildren_SameName(this Component pTarget, string strObjectName, Type pComponentType, bool bInclude_OnDisable)
    {
        List<UnityEngine.Object> listComponent = GetComponentsInChildrenList_SameName(pTarget, strObjectName, pComponentType, bInclude_OnDisable);
        if (listComponent.Count > 0)
            return listComponent[0];
        return null;
    }

    public static UnityEngine.Object[] GetComponentsInChildren_SameName(this Component pTarget, string strObjectName, Type pComponentType, bool bInclude_OnDisable)
    {
        return GetComponentsInChildrenList_SameName(pTarget, strObjectName, pComponentType, bInclude_OnDisable).ToArray();
    }

    public static List<UnityEngine.Object> GetComponentsInChildrenList_SameName(this Component pTarget, string strObjectName, Type pComponentType, bool bInclude_OnDisable)
    {
        Component[] arrComponentFind = null;
        if (pComponentType == typeof(GameObject))
            arrComponentFind = pTarget.transform.GetComponentsInChildren(typeof(Transform), true);
        else
            arrComponentFind = pTarget.transform.GetComponentsInChildren(pComponentType, bInclude_OnDisable);

        return ExtractSameNameList(strObjectName, arrComponentFind);
    }

    public static List<UnityEngine.Object> ExtractSameNameList(string strObjectName, UnityEngine.Object[] arrComponentFind)
    {
        List<UnityEngine.Object> listReturn = new List<UnityEngine.Object>();
        if (arrComponentFind != null)
        {
            for (int i = 0; i < arrComponentFind.Length; i++)
            {
                if (arrComponentFind[i].name.Equals(strObjectName))
                    listReturn.Add(arrComponentFind[i]);
            }
        }

        return listReturn;
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

    public static bool CheckValueIsNull(this MemberInfo pMemberInfo, object pTarget)
    {
        FieldInfo pFieldInfo = pMemberInfo as FieldInfo;
        if (pFieldInfo == null)
            return true;

        object pObjectValue = pFieldInfo.GetValue(pTarget);
        if (pObjectValue == null)
            return true;

        return false;
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

    public static object GetValue_Extension(this MemberInfo pMemberInfo, object pTarget)
    {
        FieldInfo pFieldInfo = pMemberInfo as FieldInfo;
        if (pFieldInfo != null)
            return pFieldInfo.GetValue(pTarget);

        PropertyInfo pProperty = pMemberInfo as PropertyInfo;
        if (pProperty != null)
            return pProperty.GetValue(pTarget);

        return null;
    }
}

#endregion