using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class UIActionAttribute_Test
    {
        // 더미 클릭 이벤트 데이터
        PointerEventData _pPointerEventData = new PointerEventData(null);
        


        public class HasUIButtonTester : MonoBehaviour
        {
            public enum EButtonName
            {
                Button_A,
                Button_B,
            }

            public int iButton_A_ClickCount { get; private set; }

            public Button pButtonClicked { get; private set; }

            public void Reset()
            {
                iButton_A_ClickCount = 0;
                pButtonClicked = null;
            }

            [UIButtonCall(EButtonName.Button_A)]
            public void CallButonA_1()
            {
                iButton_A_ClickCount++;
            }

            [UIButtonCall(nameof(EButtonName.Button_A))]
            public void CallButonA_2()
            {
                iButton_A_ClickCount++;
            }

            [UIButtonCall(EButtonName.Button_B)]
            public void CallButonB(Button pButtonInstance)
            {
                pButtonClicked = pButtonInstance;
            }
        }

        [Test]
        public void 같은이름의_인자가없는_버튼의_기능동작테스트()
        {
            // Arrange (데이터 정렬)
            GameObject pObjectTester = new GameObject(nameof(같은이름의_인자가없는_버튼의_기능동작테스트));
            Button pButtonA = Create_ChildComponent<Button>(pObjectTester, HasUIButtonTester.EButtonName.Button_A.ToString());
            Create_ChildComponent<Button>(pObjectTester, HasUIButtonTester.EButtonName.Button_B.ToString());


            HasUIButtonTester pTester = pObjectTester.AddComponent<HasUIButtonTester>();
            pTester.Reset();
            Assert.AreEqual(pTester.iButton_A_ClickCount, 0);



            // Act (실행)
            UIActionAttributeSetter.DoSet_UIActionAttribute(pTester);
            // 스크립트로 수동으로 버튼 A 클릭
            pButtonA.OnPointerClick(_pPointerEventData);



            // Assert(맞는지 체크)
            Assert.AreEqual(pTester.iButton_A_ClickCount, 2);
        }


        [Test]
        public void 인자가1개있는_버튼의_기능동작테스트()
        {
            // Arrange (데이터 정렬)
            GameObject pObjectTester = new GameObject(nameof(인자가1개있는_버튼의_기능동작테스트));
            Button pButtonB = Create_ChildComponent<Button>(pObjectTester, HasUIButtonTester.EButtonName.Button_B.ToString());
            Create_ChildComponent<Button>(pObjectTester, HasUIButtonTester.EButtonName.Button_A.ToString());

            HasUIButtonTester pTester = pObjectTester.AddComponent<HasUIButtonTester>();
            pTester.Reset();
            Assert.IsNull(pTester.pButtonClicked);



            // Act (실행)
            UIActionAttributeSetter.DoSet_UIActionAttribute(pTester);
            // 스크립트로 수동으로 버튼 B 클릭
            pButtonB.OnPointerClick(_pPointerEventData);



            // Assert(맞는지 체크)
            Assert.AreEqual(pTester.pButtonClicked, pButtonB);
        }

        // ====================================================================================== //


        public class HasUIToggleTester : MonoBehaviour
        {
            public enum EToggleName
            {
                Toggle_A,
                Toggle_B,
            }

            public int iToggle_A_ClickCount { get; private set; }
            public bool bToggleA_Input { get; private set; }
            public bool bToggleB_Input { get; private set; }

            public Toggle pToggleB { get; private set; }

            public void Reset()
            {
                bToggleA_Input = false;
                bToggleB_Input = false;

                iToggle_A_ClickCount = 0;
                pToggleB = null;
            }

            [UIToggleCall(EToggleName.Toggle_A)]
            public void 토글_A_콜_1(bool bToggle)
            {
                bToggleA_Input = bToggle;
                iToggle_A_ClickCount++;
            }

            [UIToggleCall(EToggleName.Toggle_A)]
            public void 토글_A_콜_2(bool bToggle)
            {
                bToggleA_Input = bToggle;
                iToggle_A_ClickCount++;
            }

            [UIToggleCall(EToggleName.Toggle_B)]
            public void 토글_B_콜(bool bToggle, Toggle pToggleInstance)
            {
                bToggleB_Input = bToggle;
                pToggleB = pToggleInstance;
            }
        }

        [Test]
        public void 같은이름의_인자가_1개있는_토글의_기능동작테스트()
        {
            // Arrange (데이터 정렬)
            GameObject pObjectTester = new GameObject(nameof(같은이름의_인자가_1개있는_토글의_기능동작테스트));
            Toggle pToggleA = Create_ChildComponent<Toggle>(pObjectTester, HasUIToggleTester.EToggleName.Toggle_A.ToString());
            Create_ChildComponent<Toggle>(pObjectTester, HasUIToggleTester.EToggleName.Toggle_B.ToString());


            HasUIToggleTester pTester = pObjectTester.AddComponent<HasUIToggleTester>();
            pTester.Reset();
            Assert.AreEqual(pTester.iToggle_A_ClickCount, 0);
            bool bToggleA_Value = pToggleA.isOn;


            // Act (실행)
            UIActionAttributeSetter.DoSet_UIActionAttribute(pTester);
            // 스크립트로 수동으로 버튼 A 클릭
            pToggleA.OnPointerClick(_pPointerEventData);



            // Assert(맞는지 체크)
            Assert.AreNotEqual(bToggleA_Value, pToggleA.isOn);
            Assert.AreEqual(pTester.iToggle_A_ClickCount, 2);
        }


        [Test]
        public void 인자가_2개있는_토글의_기능동작테스트()
        {
            // Arrange (데이터 정렬)
            GameObject pObjectTester = new GameObject(nameof(인자가_2개있는_토글의_기능동작테스트));
            Toggle pToggleB = Create_ChildComponent<Toggle>(pObjectTester, HasUIToggleTester.EToggleName.Toggle_B.ToString());
            Create_ChildComponent<Toggle>(pObjectTester, HasUIToggleTester.EToggleName.Toggle_A.ToString());


            HasUIToggleTester pTester = pObjectTester.AddComponent<HasUIToggleTester>();
            pTester.Reset();
            Assert.IsNull(pTester.pToggleB);
            bool bToggleB_Value = pToggleB.isOn;


            // Act (실행)
            UIActionAttributeSetter.DoSet_UIActionAttribute(pTester);
            // 스크립트로 수동으로 버튼 A 클릭
            pToggleB.OnPointerClick(_pPointerEventData);



            // Assert(맞는지 체크)
            Assert.AreNotEqual(bToggleB_Value, pToggleB.isOn);
            Assert.AreEqual(pTester.pToggleB, pToggleB);
        }


        // ====================================================================================== //

        private static T Create_ChildComponent<T>(GameObject pObjectTester, string strComponentName)
            where T : Component
        {
            T pSomethingComponent = new GameObject(strComponentName).AddComponent<T>();
            pSomethingComponent.transform.SetParent((pObjectTester.transform));

            return pSomethingComponent;
        }
    }
}
