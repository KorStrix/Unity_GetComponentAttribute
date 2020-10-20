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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = System.Object;

public interface IUIActionAttribute
{
    string strUIElementName { get; }
}

/// <summary>
/// <see cref="Button"/>이 클릭되었을 경우 해당 함수를 자동으로 호출합니다.
/// <para>인자가 0개, 1개일 수 있으며,</para>
/// <para>인자가 1개면 <see cref="Button"/>타입의 인자 1개만 허용됩니다.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class UIToggleCallAttribute : PropertyAttribute, IUIActionAttribute
{
    public string strUIElementName => strToggleName;

    public string strToggleName { get; private set; }
    public bool bPrint_OnError { get; private set; }

    public UIToggleCallAttribute(Object ToggleName, bool bPrint_OnError = true)
    {
        strToggleName = ToggleName.ToString();
        this.bPrint_OnError = bPrint_OnError;
    }
}

/// <summary>
/// <see cref="Button"/>이 클릭되었을 경우 해당 함수를 자동으로 호출합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class UIButtonCallAttribute : PropertyAttribute, IUIActionAttribute
{
    public bool bIsInit = false;
    public string strUIElementName => strButtonName;

    public string strButtonName { get; private set; }
    public bool bPrint_OnError { get; private set; }

    public UIButtonCallAttribute(Object ButtonName, bool bPrint_OnError = true)
    {
        strButtonName = ButtonName.ToString();
        this.bPrint_OnError = bPrint_OnError;
    }
}


public static class UIActionAttributeSetter
{
    public static void DoSet_UIActionAttribute(MonoBehaviour pMono) => DoSet_UIActionAttribute(pMono, pMono);

    public static void DoSet_UIActionAttribute(MonoBehaviour pMono, object pClass_Anything)
    {
        if (pMono == null)
        {
            Debug.LogError($"{nameof(DoSet_UIActionAttribute)} - pMono == null");
            return;
        }

        if (pClass_Anything == null)
        {
            Debug.LogError($"{nameof(DoSet_UIActionAttribute)} - pClass_Anything == null");
            return;
        }



        // BindingFlags를 일일이 써야 잘 동작한다..
        Type pType = pClass_Anything.GetType();
        List<MethodInfo> listMethod = new List<MethodInfo>(pType.GetMethods(BindingFlags.Public | BindingFlags.Instance));
        listMethod.AddRange(pType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance));

        foreach (var pMember in listMethod)
            DoSet_UIActionAttribute(pMono, pClass_Anything, pMember);
    }

    public static void DoSet_UIActionAttribute(MonoBehaviour pTargetMono, object pMemberOwner, MethodInfo pMethodInfo)
    {
        if (pMethodInfo == null)
            return;

        Object[] arrCustomAttributes = pMethodInfo.GetCustomAttributes(true);
        if (arrCustomAttributes.Length == 0)
            return;

        Init_ButtonCallAttribute(pTargetMono, pMemberOwner, pMethodInfo, arrCustomAttributes.OfType<UIButtonCallAttribute>().ToArray());
        Init_ToggleCallAttribute(pTargetMono, pMemberOwner, pMethodInfo, arrCustomAttributes.OfType<UIToggleCallAttribute>().ToArray());
    }

    private static void Init_ToggleCallAttribute(MonoBehaviour pTargetMono, object pMemberOwner, MethodInfo pMethodInfo, UIToggleCallAttribute[] arrCustomAttributes)
    {
        if (arrCustomAttributes.Length == 0)
            return;

        Toggle[] arrHasToggles = GetComponentsInChildren_ComponentArray<Toggle>(pTargetMono);
        if (arrHasToggles.Length == 0)
        {
            Debug.LogError($"{nameof(UIToggleCallAttribute)} - {pTargetMono.name} - {pMethodInfo.Name} - Not has Toggle in Children Object", pTargetMono);
            return;
        }


        foreach (UIToggleCallAttribute pAttribute in arrCustomAttributes)
        {
            Toggle[] arrEqualNameComponent = arrHasToggles.Where(p => p.name.Equals(pAttribute.strUIElementName)).ToArray();
            if (arrEqualNameComponent.Any() == false)
            {
                Debug.LogError($"{nameof(UIToggleCallAttribute)} - {pTargetMono.name} - {pMethodInfo.Name} - Not has Match Name({pAttribute.strUIElementName}) Toggle in Children Object", pTargetMono);
                continue;
            }


            bool bIsWrongParameter = false;
            ParameterInfo[] arrParam = pMethodInfo.GetParameters();
            if (arrParam.Length == 1)
            {
                // 중복체크를 위해 Delegate Instance 생성 (RemoveListener)
                UnityAction<bool> pUnityAction = (UnityAction<bool>)pMethodInfo.CreateDelegate(typeof(UnityAction<bool>), pMemberOwner);

                foreach (var pToggle in arrEqualNameComponent)
                {
                    pToggle.onValueChanged.RemoveListener(pUnityAction);
                    pToggle.onValueChanged.AddListener(pUnityAction);
                }
            }

            // 인자가 있을 경우 onClick의 매개변수와 일치하지 않기때문에
            // 어디에 이미 onClick에 등록했다는 정보를 저장하지 않는 이상
            // 람다만으로는 onClick 중복체크가 불가능

            //else if (arrParam.Length == 2) // 인자가 2개면 인풋값과 함께 인스턴스를 보내기로
            //{
            //    if (arrParam[0].ParameterType == typeof(bool) && arrParam[1].ParameterType == typeof(Toggle))
            //    {
            //        foreach (var pToggle in arrEqualNameComponent)
            //            pToggle.onValueChanged.AddListener((bToggle) => pMethodInfo.Invoke(pMemberOwner, new object[] {bToggle, pToggle}));
            //    }
            //    else if (arrParam[0].ParameterType == typeof(Toggle) && arrParam[1].ParameterType == typeof(bool))
            //    {
            //        foreach (var pToggle in arrEqualNameComponent)
            //            pToggle.onValueChanged.AddListener((bToggle) => pMethodInfo.Invoke(pMemberOwner, new object[] {pToggle, bToggle}));
            //    }
            //    else
            //        bIsWrongParameter = true;
            //}
            else
                bIsWrongParameter = true;

            if (bIsWrongParameter)
                Debug.LogError($"{nameof(UIToggleCallAttribute)} - {pTargetMono.name} - {pMethodInfo.Name} - Wrong Parameter. support parameter type is (bool)", pTargetMono);
        }
    }

    private static void Init_ButtonCallAttribute(MonoBehaviour pTargetMono, object pMemberOwner, MethodInfo pMethodInfo, UIButtonCallAttribute[] arrCustomAttributes)
    {
        if (arrCustomAttributes.Length == 0)
            return;

        Button[] arrHasButtons = GetComponentsInChildren_ComponentArray<Button>(pTargetMono);
        if (arrHasButtons.Length == 0)
        {
            Debug.LogError($"{nameof(UIButtonCallAttribute)} - {pTargetMono.name} - {pMethodInfo.Name} - Not has Button in Children Object", pTargetMono);
            return;
        }


        foreach (UIButtonCallAttribute pAttribute in arrCustomAttributes)
        {
            Button[] arrEqualNameComponent = arrHasButtons.Where(p => p.name.Equals(pAttribute.strUIElementName)).ToArray();
            if (arrEqualNameComponent.Any() == false)
            {
                Debug.LogError($"{nameof(UIButtonCallAttribute)} - {pTargetMono.name} - {pMethodInfo.Name} - Not has Match Name({pAttribute.strUIElementName}) Button in Children Object", pTargetMono);
                continue;
            }


            ParameterInfo[] arrParam = pMethodInfo.GetParameters();
            if (arrParam.Length == 0)
            {
                // 중복체크를 위해 Delegate Instance 생성 (RemoveListener)
                UnityAction pUnityAction = (UnityAction)pMethodInfo.CreateDelegate(typeof(UnityAction), pMemberOwner);

                foreach (var pButton in arrEqualNameComponent)
                {
                    pButton.onClick.RemoveListener(pUnityAction);
                    pButton.onClick.AddListener(pUnityAction);
                }
            }

            // 인자가 있을 경우 onClick의 매개변수와 일치하지 않기때문에
            // 어디에 이미 onClick에 등록했다는 정보를 저장하지 않는 이상
            // 람다만으로는 onClick 중복체크가 불가능

            //else if (arrParam.Length == 1 && arrParam[0].ParameterType == typeof(Button)) // 일단 인자가 1개면 버튼 인스턴스를 보내기로
            //{
            //    foreach (var pButton in arrEqualNameComponent)
            //    {
            //        pButton.onClick.AddListener(() => pMethodInfo.Invoke(pMemberOwner, new object[] { pButton }));
            //    }
            //}
            else
                Debug.LogError($"{nameof(UIButtonCallAttribute)} - {pTargetMono.name} - {pMethodInfo.Name} - Wrong Parameter. support parameter type is ()", pTargetMono);

            pAttribute.bIsInit = true;
        }
    }

    static T[] GetComponentsInChildren_ComponentArray<T>(MonoBehaviour pTarget)
        where T : Component
    {
        return pTarget.transform.GetComponentsInChildren(typeof(T), true).
            OfType<T>().
            ToArray();
    }
}

public static class UnityEventExtension
{
    public static int GetListenerNumber(this UnityEventBase unityEvent)
    {
        var field = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        var invokeCallList = field.GetValue(unityEvent);
        var property = invokeCallList.GetType().GetProperty("Count");
        return (int)property.GetValue(invokeCallList);
    }
}
