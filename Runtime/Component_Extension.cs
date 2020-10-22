#region Header
/*	============================================
 *	Aurthor 			  : Strix
 *	Initial Creation Date : 2020-10-22 오후 5:33:21
 *	Summary 			  : 
 *  Template 		      : Visual Studio ItemTemplate For Unity V7
   ============================================ */
#endregion Header

using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 혹여나 플러그인 밖에서 <see cref="GetComponentAttribute"/> 관련 (예시로 이름으로 <see cref="Component"/> 찾기 등) 기능이 필요할 경우
/// <para>편히 쓰게 하기 위한 <see cref="Component"/>용 확장 class</para>
/// </summary>
public static class Component_Extension
{
    /// <summary>
    /// <see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/> 호출 후 매개변수로 넣는 이름과 같은 오브젝트를 찾습니다.
    /// </summary>
    /// <param name="pTarget">GetComponent의 기준이 되는 Target</param>
    /// <param name="strObjectName">찾고자 하는 이름</param>
    /// <param name="pComponentType">찾고자 하는 타입</param>
    /// <param name="bInclude_DisableObject">Disable된 오브젝트를 포함할 지</param>
    /// <returns>자식 오브젝트 중 이름과 타입이 같은 컴포넌트</returns>
    public static Component GetComponentInChildren_SameName(this Component pTarget, string strObjectName, Type pComponentType, bool bInclude_DisableObject) => GetComponentsInChildrenArray_SameName(pTarget, strObjectName, pComponentType, bInclude_DisableObject).FirstOrDefault();

    /// <summary>
    /// <see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/> 호출 후 매개변수로 넣는 이름과 같은 오브젝트 '들을' 찾습니다.
    /// </summary>
    /// <param name="pTarget">GetComponent의 기준이 되는 Target</param>
    /// <param name="strObjectName">찾고자 하는 이름</param>
    /// <param name="pComponentType">찾고자 하는 타입</param>
    /// <param name="bInclude_DisableObject">Disable된 오브젝트를 포함할 지</param>
    /// <returns>자식 오브젝트 중 이름과 타입이 같은 컴포넌트들</returns>
    public static Component[] GetComponentsInChildrenArray_SameName(this Component pTarget, string strObjectName, Type pComponentType, bool bInclude_DisableObject)
    {
        Component[] arrComponentFind = null;
        if (pComponentType == typeof(GameObject))
            arrComponentFind = pTarget.transform.GetComponentsInChildren(typeof(Transform), bInclude_DisableObject);
        else
            arrComponentFind = pTarget.transform.GetComponentsInChildren(pComponentType, bInclude_DisableObject);

        return ExtractSameNameArray(strObjectName, arrComponentFind);
    }

    static Component[] _EmptyComponentArray = new Component[0];

    /// <summary>
    /// 같은 이름의 컴포넌트를 찾아 리턴합니다.
    /// </summary>
    /// <param name="strObjectName">찾고자 하는 이름</param>
    /// <param name="arrComponentFind">찾아야 할 컬렉션</param>
    /// <returns>찾은 컴포넌트 Array // 없으면 Length는 0입니다.</returns>
    public static Component[] ExtractSameNameArray(string strObjectName, Component[] arrComponentFind)
    {
        if (arrComponentFind == null)
            return _EmptyComponentArray;

        return arrComponentFind.Where(p => p.name.Equals(strObjectName)).ToArray();
    }
}
