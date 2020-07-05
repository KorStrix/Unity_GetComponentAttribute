using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class UIButtonAttribute_Test
    {
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
            public void 버튼_A_콜_1()
            {
                iButton_A_ClickCount++;
            }

            [UIButtonCall(nameof(EButtonName.Button_A))]
            public void 버튼_A_콜_2()
            {
                iButton_A_ClickCount++;
            }

            [UIButtonCall(EButtonName.Button_B)]
            public void 버튼_B_콜(Button pButtonInstance)
            {
                pButtonClicked = pButtonInstance;
            }

        }

        // ====================================================================================== //

        [Test]
        public void 같은이름의_인자가없는_버튼의_기능동작테스트()
        {
            // Arrange (데이터 정렬)
            GameObject pObjectTester = new GameObject(nameof(같은이름의_인자가없는_버튼의_기능동작테스트));
            Button pButtonA = Create_ChildButton(pObjectTester, HasUIButtonTester.EButtonName.Button_A.ToString());
            Create_ChildButton(pObjectTester, HasUIButtonTester.EButtonName.Button_B.ToString());

            // 더미 클릭 이벤트 데이터
            PointerEventData pPointerEventData = new PointerEventData(null);

            HasUIButtonTester pTester = pObjectTester.AddComponent<HasUIButtonTester>();
            pTester.Reset();
            Assert.AreEqual(pTester.iButton_A_ClickCount, 0);



            // Act (실행)
            UIButtonAttributeSetter.DoUpdate_UIButtonAttribute(pTester);
            // 스크립트로 수동으로 버튼 A 클릭
            pButtonA.OnPointerClick(pPointerEventData);



            // Assert(맞는지 체크)
            Assert.AreEqual(pTester.iButton_A_ClickCount, 2);
        }


        [Test]
        public void 인자가1개있는_버튼의_기능동작테스트()
        {
            // Arrange (데이터 정렬)
            GameObject pObjectTester = new GameObject(nameof(인자가1개있는_버튼의_기능동작테스트));
            Button pButtonB = Create_ChildButton(pObjectTester, HasUIButtonTester.EButtonName.Button_B.ToString());
            Create_ChildButton(pObjectTester, HasUIButtonTester.EButtonName.Button_A.ToString());

            // 더미 클릭 이벤트 데이터
            PointerEventData pPointerEventData = new PointerEventData(null);

            HasUIButtonTester pTester = pObjectTester.AddComponent<HasUIButtonTester>();
            pTester.Reset();
            Assert.IsNull(pTester.pButtonClicked);



            // Act (실행)
            UIButtonAttributeSetter.DoUpdate_UIButtonAttribute(pTester);
            // 스크립트로 수동으로 버튼 A 클릭
            pButtonB.OnPointerClick(pPointerEventData);



            // Assert(맞는지 체크)
            Assert.AreEqual(pTester.pButtonClicked, pButtonB);
        }

        // ====================================================================================== //

        private static Button Create_ChildButton(GameObject pObjectTester, string strButtonName)
        {
            Button pSomethingButton = new GameObject(strButtonName).AddComponent<Button>();
            pSomethingButton.transform.SetParent((pObjectTester.transform));

            return pSomethingButton;
        }
    }
}
