#region Header
/*	============================================
 *	Author 			    	: Strix
 *	Initial Creation Date 	: 2020-07-05
 *	Summary 		        : 
 *  Template 		        : New Behaviour For Unity Editor V2
   ============================================ */
#endregion Header

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class UIButtonCallAttribute : PropertyAttribute
{
    public string strButtonName { get; private set; } = "";
    public bool bPrint_OnError { get; private set; } = true;

    public UIButtonCallAttribute(string strButtonName, bool bPrint_OnError = true)
    {
        this.strButtonName = strButtonName;
        this.bPrint_OnError = bPrint_OnError;
    }

    public UIButtonCallAttribute(object pObject, bool bPrint_OnError = true)
    {
        strButtonName = pObject.ToString();
        this.bPrint_OnError = bPrint_OnError;
    }
}

public static class UIButtonAttributeSetter
{
    public static void DoUpdate_UIButtonAttribute(MonoBehaviour pMono) => DoUpdate_UIButtonAttribute(pMono, pMono);

    public static void DoUpdate_UIButtonAttribute(MonoBehaviour pMono, object pClass_Anything)
    {
        // BindingFlags를 일일이 써야 잘 동작한다..
        Type pType = pClass_Anything.GetType();
        List<MethodInfo> listMembers = new List<MethodInfo>(pType.GetMethods(BindingFlags.Public | BindingFlags.Instance));
        listMembers.AddRange(pType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance));

        var arrMembers_Filtered = listMembers.Where(p => p.GetCustomAttributes().Any());
        foreach (var pMember in arrMembers_Filtered)
            DoUpdate_UIButtonAttribute(pMono, pClass_Anything, pMember);
    }

    public static void DoUpdate_UIButtonAttribute(MonoBehaviour pTargetMono, object pMemberOwner, MethodInfo pMethodInfo)
    {
        if (pMethodInfo == null)
            return;


        UIButtonCallAttribute[] arrCustomAttributes = pMethodInfo.GetCustomAttributes(true).
            OfType<UIButtonCallAttribute>().
            ToArray();
        if (arrCustomAttributes.Length == 0)
            return;

        Button[] arrHasButtons = GetComponentsInChildren_ButtonArray(pTargetMono);
        if (arrHasButtons.Length == 0)
        {
            Debug.LogError($"{nameof(UIButtonCallAttribute)} - {pTargetMono.name} - {pMethodInfo.Name} - Not has Button in Children Object", pTargetMono);
            return;
        }

        foreach (UIButtonCallAttribute pAttribute in arrCustomAttributes)
        {
            Button[] arrEqualNameButton = arrHasButtons.Where(p => p.name.Equals(pAttribute.strButtonName)).ToArray();
            if (arrEqualNameButton.Any() == false)
            {
                Debug.LogError($"{nameof(UIButtonCallAttribute)} - {pTargetMono.name} - {pMethodInfo.Name} - Not has Match Name({pAttribute.strButtonName}) Button in Children Object", pTargetMono);
                continue;
            }

            var arrParam = pMethodInfo.GetParameters();
            if (arrParam.Length == 0)
            {
                foreach (var pButton in arrEqualNameButton)
                    pButton.onClick.AddListener(() => pMethodInfo.Invoke(pTargetMono, null));
            }
            else if(arrParam.Length == 1) // 일단 인자가 1개면 버튼 인스턴스를 보내기로
            {
                foreach (var pButton in arrEqualNameButton)
                    pButton.onClick.AddListener(() => pMethodInfo.Invoke(pTargetMono, new object[] { pButton }));
            }
        }
    }

    static Button[] GetComponentsInChildren_ButtonArray(MonoBehaviour pTarget)
    {
        return pTarget.transform.GetComponentsInChildren(typeof(Button), true).
            OfType<Button>().
            ToArray();
    }
}