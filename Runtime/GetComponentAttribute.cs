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
 *  GetComponentAttribute관련 코드는 일부러 단일 파일에 관리하고 있습니다.
 *  - Custom Package를 지원하지 않는 유니티 저버전에서 설치하기 용이하기 위함
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
using GetComponentAttributeCore;
using Object = System.Object;

namespace GetComponentAttributeCore
{
    /// <summary>
    /// 이 네임스페이스의 Root Base Class
    /// <para>목표는 <see cref="GetComponentAttributeSetter"/>가 <see cref="MonoBehaviour"/>의 변수/프로퍼티를 이 Attribute를 통해 할당하는 것입니다.</para>
    /// </summary>
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

    public interface IGetComponentChildrenAttribute : IGetComponentAttribute
    {
        bool bSearch_By_ComponentName_ForGetComponent { get; }
        string strComponentName_ForGetComponent { get; }
    }

    public static class CoreLogic
    {
        #region zero array는 필요시마다 new를 생성하기 보단 캐싱해서 사용

        static readonly UnityEngine.Object[] _EmptyUnityObjectArray = new UnityEngine.Object[0];
        static readonly GameObject[] _EmptyGameObjectArray = new GameObject[0];
        static readonly Object[] _EmptyObjectArray = new Object[0];
        static readonly Type[] _EmptyTypeArray = new Type[0];
        static readonly Type[] _BoolTypeArray = new[] { typeof(bool) };

        #endregion

        public static object Event_GetComponent(MonoBehaviour pMono, Type pElementType)
        {
            // ReSharper disable once PossibleNullReferenceException
            MethodInfo getter = typeof(MonoBehaviour)
                     .GetMethod("GetComponents", _EmptyTypeArray)
                     .MakeGenericMethod(pElementType);

            return getter.Invoke(pMono, null);
        }

        public static object Event_GetComponentInChildren(MonoBehaviour pMono, Type pElementType, bool bInclude_DeActive, bool bSearch_By_ComponentName, string strComponentName)
        {
            MethodInfo pGetMethod = typeof(MonoBehaviour).GetMethod("GetComponentsInChildren", _BoolTypeArray);

            if (pElementType.HasElementType)
                pElementType = pElementType.GetElementType();

            object pObjectReturn;
            if (pElementType == typeof(GameObject))
            {
                pElementType = typeof(Transform);
                // ReSharper disable once PossibleNullReferenceException
                pGetMethod = pGetMethod.MakeGenericMethod(pElementType);
                pObjectReturn = Convert_TransformArray_To_GameObjectArray(pGetMethod.Invoke(pMono, new object[] { bInclude_DeActive }));
            }
            else
            {
                // ReSharper disable once PossibleNullReferenceException
                pGetMethod = pGetMethod.MakeGenericMethod(pElementType);
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
                GetMethod("GetComponentsInParent", _EmptyTypeArray).
                MakeGenericMethod(pElementType);

            if (bTypeIsGameObject)
                return Convert_TransformArray_To_GameObjectArray(pGetMethod.Invoke(pTargetMono, _EmptyObjectArray));
            return pGetMethod.Invoke(pTargetMono, _EmptyObjectArray);
        }

        public static UnityEngine.Object[] ExtractSameNameArray(string strObjectName, UnityEngine.Object[] arrComponentFind)
        {
            if (arrComponentFind == null)
                return _EmptyUnityObjectArray;

            return arrComponentFind.Where(p => p.name.Equals(strObjectName)).ToArray();
        }


        private static GameObject[] Convert_TransformArray_To_GameObjectArray(object pObject)
        {
            Transform[] arrTransform = pObject as Transform[];
            if (arrTransform == null)
                return _EmptyGameObjectArray;

            return arrTransform.Select(p => p.gameObject).ToArray();
        }
    }
}

/// <summary>
/// <see cref="GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(MonoBehaviour)"/>함수 호출을 통해 
/// <para><see cref="UnityEngine.Component.GetComponents(Type)"/>으로 변수/프로퍼티를 할당합니다.</para>
/// </summary>
public class GetComponentAttribute : GetComponentAttributeBase
{
    public override object GetComponent(MonoBehaviour pMono, Type pElementType)
    {
        return CoreLogic.Event_GetComponent(pMono, pElementType);
    }
}

/// <summary>
/// <see cref="GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(MonoBehaviour)"/>함수 호출을 통해 
/// <para><see cref="UnityEngine.Component.GetComponentInParent(Type)"/>으로 변수/프로퍼티를 할당합니다.</para>
/// </summary>
public class GetComponentInParentAttribute : GetComponentAttributeBase
{
    public override object GetComponent(MonoBehaviour pMono, Type pElementType)
    {
        return CoreLogic.Event_GetComponentInParents(pMono, pElementType);
    }
}


/// <summary>
/// <see cref="GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(MonoBehaviour)"/>함수 호출을 통해 
/// <para><see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/>으로 변수/프로퍼티를 할당합니다.</para>
/// <remarks>성능을 우선한다면 <see cref="bInclude_DisableObject"/>값을 <see langword="false"/>로 하세요.</remarks>
/// </summary>
/// 
/// <example>
/// 다음은 사용 예시입니다.
/// <code>
/// [GetComponentInChildren()]
/// public Rigidbody _rigidbody = null;
///
///
/// Find ObjectName Component Example
/// enum SomeEnum { First, Second }
/// 
/// [GetComponentInChildren(SomeEnum.First)]
/// public Rigidbody _rigidbody = null;
///
/// [GetComponentInChildren("Second")]
/// public Rigidbody _rigidbody { get; private set; }
///
///
/// List Example
/// [GetComponentInChildren]
/// public List(Rigidbody) _rigidbodies = null; // = { Rigidbody(First) }, { Rigidbody(Second) }
///
/// Dictionary Example
/// [GetComponentInChildren]
/// public Dictionary(SomeEnum, Rigidbody) _rigidbodies {get; private set;} // = { First, Rigidbody } , { Second, Rigidbody }
/// </code>
/// </example>
public class GetComponentInChildrenAttribute : GetComponentAttributeBase, IGetComponentChildrenAttribute
{
    public bool bSearch_By_ComponentName_ForGetComponent => bSearch_By_ComponentName;
    public string strComponentName_ForGetComponent => strComponentName;


    public bool bSearch_By_ComponentName;
    public bool bInclude_DisableObject;
    public string strComponentName;

    /// <summary>
    /// <see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/>을 호출하여 자식 컴포넌트를 찾아 할당합니다.
    /// </summary>
    /// <param name="bInclude_DisableObject">Disable 된 오브젝트까지 포함할 지</param>
    public GetComponentInChildrenAttribute(bool bInclude_DisableObject = true)
    {
        bSearch_By_ComponentName = false;
        this.bInclude_DisableObject = bInclude_DisableObject;
    }

    /// <summary>
    /// <see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/>을 호출하여 자식 컴포넌트를 찾아 할당합니다.
    /// </summary>
    /// <param name="bInclude_DisableObject">Disable 된 오브젝트까지 포함할 지</param>
    /// <param name="bIsPrint_OnNotFound">오브젝트를 못찾았을 경우 <see cref="Debug.LogError(Object)"/>를 출력할 지</param>
    public GetComponentInChildrenAttribute(bool bInclude_DisableObject, bool bIsPrint_OnNotFound = true)
    {
        this.bInclude_DisableObject = bInclude_DisableObject;
        bSearch_By_ComponentName = false;
        this.bIsPrint_OnNotFound = bIsPrint_OnNotFound;
    }

    /// <summary>
    /// <see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/>을 호출하여 자식 컴포넌트를 찾아 할당합니다.
    /// </summary>
    /// <param name="pFindComponentName">여기엔 <see cref="string"/>혹은 <see cref="System.Enum"/> 타입만 들어가야 합니다. 찾고자 하는 컴포넌트 이름입니다.</param>
    public GetComponentInChildrenAttribute(Object pFindComponentName)
    {
        bInclude_DisableObject = true;
        strComponentName = pFindComponentName.ToString();
        bSearch_By_ComponentName = true;
    }

    /// <summary>
    /// <see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/>을 호출하여 자식 컴포넌트를 찾아 할당합니다.
    /// </summary>
    /// <param name="pFindComponentName">여기엔 <see cref="string"/>혹은 <see cref="System.Enum"/> 타입만 들어가야 합니다. 찾고자 하는 컴포넌트 이름입니다.</param>
    /// <param name="bInclude_DisableObject">Disable 된 오브젝트까지 포함할 지</param>
    public GetComponentInChildrenAttribute(Object pFindComponentName, bool bInclude_DisableObject)
    {
        strComponentName = pFindComponentName.ToString();
        bSearch_By_ComponentName = true;
        this.bInclude_DisableObject = bInclude_DisableObject;
    }

    /// <summary>
    /// <see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/>을 호출하여 자식 컴포넌트를 찾아 할당합니다.
    /// </summary>
    /// <param name="pFindComponentName">여기엔 <see cref="string"/>혹은 <see cref="System.Enum"/> 타입만 들어가야 합니다. 찾고자 하는 컴포넌트 이름입니다.</param>
    /// <param name="bInclude_DisableObject">Disable 된 오브젝트까지 포함할 지</param>
    /// <param name="bIsPrint_OnNotFound">오브젝트를 못찾았을 경우 <see cref="Debug.LogError(Object)"/>를 출력할 지</param>
    public GetComponentInChildrenAttribute(Object pFindComponentName, bool bInclude_DisableObject, bool bIsPrint_OnNotFound = true)
    {
        this.bInclude_DisableObject = bInclude_DisableObject;

        strComponentName = pFindComponentName.ToString();
        bSearch_By_ComponentName = true;
        this.bIsPrint_OnNotFound = bIsPrint_OnNotFound;
    }

    public override object GetComponent(MonoBehaviour pMono, Type pElementType)
    {
        return CoreLogic.Event_GetComponentInChildren(pMono, pElementType, bInclude_DisableObject, bSearch_By_ComponentName, strComponentName);
    }
}


/// <summary>
/// <see cref="GetComponentAttribute"/>가 있는 필드 / 프로퍼티를 할당시켜주는 <see langword="static"/> class
/// </summary>
public static class GetComponentAttributeSetter
{
    private static Dictionary<Type, List<MemberInfo>> _mapMemberInfo_Cached = new Dictionary<Type, List<MemberInfo>>();

    /// <summary>
    /// 매개변수로 넣는 모노비헤비어의 <see cref="GetComponentAttribute"/>를 붙인 필드/프로퍼티를 모두 찾아 할당합니다.
    /// </summary>
    /// <param name="pMonoTarget"><see cref="GetComponentAttribute"/>를 실행할 대상</param>
    public static void DoUpdate_GetComponentAttribute(MonoBehaviour pMonoTarget) =>  DoUpdate_GetComponentAttribute(pMonoTarget, pMonoTarget);

    public static void DoClearCached()
    {
        _mapMemberInfo_Cached.Clear();
    }

    /// <summary>
    /// 매개변수로 넣는 클래스(아무 상속받지 않은 class도 가능)에서
    /// <para><see cref="GetComponentAttribute"/>를 붙인 필드/프로퍼티를 매개변수로 넣은 모노비헤비어 기준으로 찾아 할당합니다.</para>
    /// </summary>
    /// <param name="pMonoTarget"><see cref="GetComponentAttribute"/>로 찾는 기준 대상</param>
    /// <param name="pClass_AttributeOwner"><see cref="GetComponentAttribute"/>를 실행할 대상</param>
    public static void DoUpdate_GetComponentAttribute(MonoBehaviour pMonoTarget, object pClass_AttributeOwner)
    {
        Type pType = pClass_AttributeOwner.GetType();
        if (_mapMemberInfo_Cached.TryGetValue(pType, out var listMember) == false)
        {
            listMember = new List<MemberInfo>();
            _mapMemberInfo_Cached.Add(pType, listMember);

            // BindingFlags를 일일이 써야 잘 동작한다..
            // _listMemberTemp.AddRange(pType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
            // _listMemberTemp.AddRange(pType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
            // _listMemberTemp.AddRange(pType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
            // _listMemberTemp.AddRange(pType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance));

            // 위같았던 기억이 나는데 다시 이거로하니깐 잘됨;
            // 퍼포먼스 문제때문에 최대한 줄이는게 맞음
            listMember.AddRange(pType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
            listMember.AddRange(pType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
        }


        foreach (var pMember in listMember)
            DoUpdate_GetComponentAttribute(pMonoTarget, pClass_AttributeOwner, pMember);

        if(pMonoTarget.gameObject.activeInHierarchy)
            pMonoTarget.StartCoroutine(ClearCacheCoroutine(pType));
    }

    static IEnumerator ClearCacheCoroutine(Type pType)
    {
        yield return new WaitForSeconds(10f);

        if (_mapMemberInfo_Cached.TryGetValue(pType, out var list))
        {
            list.Clear();
            _mapMemberInfo_Cached.Remove(pType);
        }
    }

    /// <summary>
    /// 매개변수로 넣는 클래스(아무 상속받지 않은 class도 가능)에서
    /// <para><see cref="GetComponentAttribute"/>를 붙인 필드/프로퍼티를 매개변수로 넣은 모노비헤비어 기준으로 찾아 할당합니다.</para>
    /// </summary>
    public static void DoUpdate_GetComponentAttribute(MonoBehaviour pMonoTarget, object pClass_AttributeOwner, MemberInfo pMemberInfo)
    {
        if (pMemberInfo == null)
            return;

        Object[] arrAttributes = pMemberInfo.GetCustomAttributes(true);
        if (arrAttributes.Length == 0)
            return;

        Type pMemberType = pMemberInfo.MemberType();
        IEnumerable<IGetComponentAttribute> arrCustomAttributes = arrAttributes.OfType<IGetComponentAttribute>();
        foreach(var pGetComponentAttribute in arrCustomAttributes)
        {
            object pComponent = SetMember_FromGetComponent(pMonoTarget, pClass_AttributeOwner, pMemberInfo, pMemberType, pGetComponentAttribute);
            if (pComponent != null)
                continue;

            if (pGetComponentAttribute.bIsPrint_OnNotFound_GetComponent)
            {
                if (pGetComponentAttribute is GetComponentInChildrenAttribute pAttribute && pAttribute.bSearch_By_ComponentName)
                    Debug.LogError(pMonoTarget.name + string.Format(".{0}<{1}>({2}) Result == null", pGetComponentAttribute.GetType().Name, pMemberType, pAttribute.strComponentName), pMonoTarget);
                else
                    Debug.LogError(pMonoTarget.name + string.Format(".{0}<{1}> Result == null", pGetComponentAttribute.GetType().Name, pMemberType), pMonoTarget);
            }
        }
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
            return pComponent;
        }


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
        if (pType_DictionaryKey == typeof(string))
        {
            if (bValue_Is_Collection)
            {
                arrChildrenComponent.OfType<UnityEngine.Object>().
                    GroupBy(p => p.name).
                    ToList().
                    ForEach(pGroup => 
                    AddDictionary_OnValueIsCollection(pMono, pType_DictionaryValue, pGroup, pTypeChild_OnValueIsCollection, Method_Add, pInstanceDictionary, (key) => key));
            }
            else
            {		
                arrChildrenComponent.OfType<UnityEngine.Object>().
                    ToList().
                    ForEach(pUnityObject => AddDictionary(pMono, Method_Add, pInstanceDictionary, pUnityObject, (key) => key));
            }
        }
        else if (pType_DictionaryKey.IsEnum)
        {
            HashSet<string> setEnumName = new HashSet<string>(Enum.GetNames(pType_DictionaryKey));

            if (bValue_Is_Collection)
            {     
                arrChildrenComponent.OfType<UnityEngine.Object>().
                    GroupBy(p => p.name).
                    Where(p => setEnumName.Contains(p.Key)).
                    ToList().
                    ForEach(pGroup =>
                    AddDictionary_OnValueIsCollection(pMono, pType_DictionaryValue, pGroup, pTypeChild_OnValueIsCollection, Method_Add, pInstanceDictionary, (key) => Enum.Parse(pType_DictionaryKey, key, true)));
            }
            else
            {
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
#if UNITY_EDITOR
        try
#endif
        {
            Method_Add.Invoke(pInstanceDictionary, new [] { OnSelectDictionaryKey(pUnityObject.name), pUnityObject});
        }
#if UNITY_EDITOR
        catch (Exception e)
        {
            Debug.LogError(pUnityObject.name + " GetComponent - GetComponent_OnDictionary - Overlap Key MonoType : " + pMono.GetType() + e, pMono);
        }
#endif
    }

    private static void AddDictionary_OnValueIsCollection(MonoBehaviour pMono, Type pType_DictionaryValue, IGrouping<string, UnityEngine.Object> pGroup, Type pTypeChild_OnValueIsCollection, MethodInfo Method_Add, object pInstanceDictionary, Func<string, object> OnSelectDictionaryKey)
    {
#if UNITY_EDITOR
        try
#endif
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
#if UNITY_EDITOR
        catch (Exception e)
        {
            Debug.LogError(e, pMono);
        }
#endif
    }
}


#region Extension

/// <summary>
/// <see cref="GetComponentAttributeSetter"/>에서 편하게 사용하기 위한 <see cref="MemberInfo"/>용 확장 클래스
/// </summary>
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