using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GetComponentAttribute_Test
{

    public class Test_ComponentChild_ForPerformance : MonoBehaviour { }

    [Category("StrixLibrary")]
    public class GetComponentAttribute_PerformanceTest : MonoBehaviour
    {
        public enum ETestChildObject
        {
            TestObject_1,
            TestObject_2,
            TestObject_3,

            TestObject_Other_FindString,
            TestObject_Other_FindEnum,

            MAX,
        }

        [GetComponentInChildren]
        private Dictionary<ETestChildObject, Test_ComponentChild_ForPerformance> p_mapTest_KeyIsEnum = new Dictionary<ETestChildObject, Test_ComponentChild_ForPerformance>();

        [GetComponentInChildren]
        private Dictionary<ETestChildObject, Transform> p_mapTransform_KeyIsEnum = new Dictionary<ETestChildObject, Transform>();

        [GetComponentInChildren]
        private Dictionary<ETestChildObject, GameObject> p_mapGameObject_KeyIsEnum = new Dictionary<ETestChildObject, GameObject>();

        [GetComponentInChildren]
        private Dictionary<string, Transform> p_mapTransform_KeyIsString = new Dictionary<string, Transform>();

        [GetComponentInChildren]
        private Dictionary<string, GameObject> p_mapGameObject_KeyIsString = new Dictionary<string, GameObject>();


        [GetComponentInChildren(nameof(ETestChildObject.TestObject_Other_FindString))]
        private Test_ComponentChild_ForPerformance p_pChildComponent_FindString = null;
        [GetComponentInChildren(ETestChildObject.TestObject_Other_FindEnum)]
        private Test_ComponentChild_ForPerformance p_pChildComponent_FindEnum = null;

        [GetComponentInChildren(nameof(ETestChildObject.TestObject_Other_FindString))]
        private GameObject p_pObject_FindString = null;
        [GetComponentInChildren(ETestChildObject.TestObject_Other_FindEnum)]
        private GameObject p_pObject_FindEnum = null;

        public void Awake()
        {
            SCGetComponentAttributeHelper.DoUpdate_GetComponentAttribute(this);
        }

        [Test]
        public static void GetComponentChildren_Field_Test()
        {
            GameObject pObjectParents = new GameObject(nameof(GetComponentChildren_Field_Test));

            // GetComponent 대상인 자식 추가
            int iChildCount = (int)ETestChildObject.MAX;
            for (int i = 0; i < iChildCount; i++)
            {
                GameObject pObjectChild = new GameObject(((ETestChildObject)i).ToString());
                pObjectChild.transform.SetParent(pObjectParents.transform);
                pObjectChild.AddComponent<Test_ComponentChild_ForPerformance>();
            }

            int iTestDummy = 5000;
            for (int i = 0; i < iTestDummy; i++)
            {
                GameObject pObjectChild = new GameObject(i.ToString());
                pObjectChild.transform.SetParent(pObjectParents.transform);
                pObjectChild.AddComponent<Test_ComponentChild_ForPerformance>();
            }


            // 자식을 전부 추가한 뒤에 페런츠에 추가한다.
            // 추가하자마자 Awake로 자식을 찾기 때문
            GetComponentAttribute_PerformanceTest pParents = pObjectParents.AddComponent<GetComponentAttribute_PerformanceTest>();

            Check_Dictionary(pParents.p_mapTest_KeyIsEnum, iChildCount);
            Check_Dictionary(pParents.p_mapTransform_KeyIsEnum, iChildCount);
            Check_Dictionary(pParents.p_mapGameObject_KeyIsEnum, iChildCount);


            //Check_Dictionary(pParents.p_mapTransform_KeyIsString, iChildCount + iTestDummy + 1);
            //Check_Dictionary(pParents.p_mapGameObject_KeyIsString, iChildCount + iTestDummy + 1);



            Assert.NotNull(pParents.p_pChildComponent_FindEnum);
            Assert.NotNull(pParents.p_pChildComponent_FindString);

            Assert.NotNull(pParents.p_pObject_FindString);
            Assert.NotNull(pParents.p_pObject_FindEnum);
        }

        private static void Check_Dictionary<TKey, TValue>(Dictionary<TKey, TValue> mapTestTarget, int iChildCount)
            where TValue : UnityEngine.Object
        {
            Assert.AreEqual(mapTestTarget.Count, iChildCount);
            var pIterEnum = mapTestTarget.GetEnumerator();
            while (pIterEnum.MoveNext())
                Assert.IsTrue(pIterEnum.Current.Key.ToString() == pIterEnum.Current.Value.name.ToString());
        }
    }
}