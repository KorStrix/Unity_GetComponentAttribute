using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

#pragma warning disable 0414

public class ChildRequireComponentAttribute_Example : MonoBehaviour {

    public enum ETestChildObject
    {
        TestObject_1,
        TestObject_2,
        TestObject_3,

        TestObject_Other_FindString,
        TestObject_Other_FindEnum,
    }

    [System.Serializable] // 인스펙터 노출용
    public class InnerClass_NotInherit_Mono
    {
        [ChildRequireComponent(nameof(ETestChildObject.TestObject_Other_FindString))]
        public Transform p_pChildComponent_FindString = null;

        [SerializeField]
        [ChildRequireComponent(ETestChildObject.TestObject_Other_FindEnum)]
        private Transform p_pChildComponent_FindEnum = null;

        [SerializeField]
        [ChildRequireComponent(ETestChildObject.TestObject_1)]
        private Transform p_pChildComponent_NotFoundCase = null;

        [SerializeField]
        [ChildRequireComponent(ETestChildObject.TestObject_1, false)]
        private Transform p_pChildComponent_NotFoundCase_Warnning = null;
    }

    public InnerClass_NotInherit_Mono p_pNotInherit_Mono;

#if ODIN_INSPECTOR
    [ShowInInspector]
#endif
    // 오딘을 지원할 경우 이너 클래스가 프로퍼티인 경우 Edit모드에선 컴포넌트를 못찾았다고 나타나나, Play하면 정상동작
    private InnerClass_NotInherit_Mono p_pNotInherit_Mono_Property { get; set; } = new InnerClass_NotInherit_Mono();


    [SerializeField]
    [ChildRequireComponent(nameof(ETestChildObject.TestObject_Other_FindString))]
    private Transform p_pChildComponent_FindString = null;

    [SerializeField]
    [ChildRequireComponent(ETestChildObject.TestObject_Other_FindEnum)]
    private Transform p_pChildComponent_FindEnum = null;

    [SerializeField]
    [ChildRequireComponent(ETestChildObject.TestObject_1)]
    private Transform p_pChildComponent_NotFoundCase = null;

    [SerializeField]
    [ChildRequireComponent(ETestChildObject.TestObject_1, false)]
    private Transform p_pChildComponent_NotFoundCase_Warnning = null;

    private void Awake()
    {
        SCManagerGetComponent.DoUpdateGetComponentAttribute(this);
        SCManagerGetComponent.DoUpdateGetComponentAttribute(this, p_pNotInherit_Mono);

        if (p_pNotInherit_Mono_Property == null)
            p_pNotInherit_Mono_Property = new InnerClass_NotInherit_Mono();
        SCManagerGetComponent.DoUpdateGetComponentAttribute(this, p_pNotInherit_Mono_Property);
    }
}
