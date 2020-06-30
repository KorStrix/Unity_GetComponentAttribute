using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GetComponentAttribute_Test
{

    public class Test_ComponentParents : MonoBehaviour { }
    public class Test_ComponentChild : MonoBehaviour { }
    public class Test_ComponentChild_SameName : MonoBehaviour { }

    public class Test_ComponentOnly : MonoBehaviour { }

    [Category("StrixLibrary")]
    public class GetComponentAttribute_Test : MonoBehaviour
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


        [GetComponentInParent]
        public Test_ComponentParents p_pParents = null;

        [GetComponentInChildren] 
        public List<Test_ComponentChild> p_listTest = new List<Test_ComponentChild>();

        [GetComponentInChildren]
        public Dictionary<string, Test_ComponentChild> p_mapTest_KeyIsString = new Dictionary<string, Test_ComponentChild>();

        [GetComponentInChildren]
        private Dictionary<ETestChildObject, Test_ComponentChild> p_mapTest_KeyIsEnum = new Dictionary<ETestChildObject, Test_ComponentChild>();

        [GetComponentInChildren(nameof(ETestChildObject.TestObject_Other_FindString))]
        private Test_ComponentChild p_pChildComponent_FindString = null;

        [GetComponentInChildren(ETestChildObject.TestObject_Other_FindEnum)]
        private Test_ComponentChild p_pChildComponent_FindEnum = null;

        [GetComponentInChildren(nameof(ETestChildObject.TestObject_Other_FindString))]
        private GameObject p_pObject_FindString = null;

        [GetComponentInChildren(ETestChildObject.TestObject_Other_FindEnum)]
        private GameObject p_pObject_FindEnum = null;


        [GetComponentInChildren] 
        public Test_ComponentChild p_pChildComponent_FindEnumProperty { get; private set; }

        [GetComponent] 
        Test_ComponentOnly[] arrComponent = null;

        [GetComponentInChildren]
        GameObject[] arrObject_Children = null;

        [GetComponentInChildren("Test")]
        public List<Test_ComponentChild_SameName> listChildren_NameIs_Test { get; private set; }

        public void Awake()
        {
            GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(this);
        }

        [Test]
        public static void GetComponentIn_Parent()
        {
            GameObject pObjectRoot = new GameObject("Root");
            pObjectRoot.AddComponent<Test_ComponentParents>();

            GameObject pObjectParents = new GameObject(nameof(GetComponentIn_Parent));
            pObjectParents.transform.SetParent(pObjectRoot.transform);

            GetComponentAttribute_Test pParents = pObjectParents.AddComponent<GetComponentAttribute_Test>();
            Assert.NotNull(pParents.p_pParents);
        }

        [Test]
        public static void GetComponentChildren_Field_Test()
        {
            GameObject pObjectParents = new GameObject(nameof(GetComponentChildren_Field_Test));

            // GetComponent 대상인 자식 추가
            int iChildCount = (int) ETestChildObject.MAX;
            for (int i = 0; i < iChildCount; i++)
            {
                GameObject pObjectChild = new GameObject(((ETestChildObject) i).ToString());
                pObjectChild.transform.SetParent(pObjectParents.transform);
                pObjectChild.AddComponent<Test_ComponentChild>();
            }

            // 자식을 전부 추가한 뒤에 페런츠에 추가한다.
            // 추가하자마자 Awake로 자식을 찾기 때문
            GetComponentAttribute_Test pParents = pObjectParents.AddComponent<GetComponentAttribute_Test>();

            // Getcomponent Attribute가 잘 작동했는지 체크 시작!!
            Assert.NotNull(pParents.p_pChildComponent_FindEnum);
            Assert.NotNull(pParents.p_pChildComponent_FindString);

            Assert.NotNull(pParents.p_pObject_FindString);
            Assert.NotNull(pParents.p_pObject_FindEnum);

            Assert.AreEqual(pParents.p_pChildComponent_FindString.name,
                ETestChildObject.TestObject_Other_FindString.ToString());
            Assert.AreEqual(pParents.p_pChildComponent_FindEnum.name,
                ETestChildObject.TestObject_Other_FindEnum.ToString());

            Assert.AreEqual(pParents.p_pObject_FindString.name,
                ETestChildObject.TestObject_Other_FindString.ToString());
            Assert.AreEqual(pParents.p_pObject_FindEnum.name, ETestChildObject.TestObject_Other_FindEnum.ToString());

            Assert.AreEqual(pParents.p_listTest.Count, iChildCount);

            Assert.AreEqual(pParents.p_mapTest_KeyIsEnum.Count, iChildCount);
            Assert.AreEqual(pParents.p_mapTest_KeyIsString.Count, iChildCount);
            Assert.AreEqual(pParents.arrObject_Children.Length,
                pObjectParents.transform.childCount + 1); // 자기 자신까지 추가하기떄문에 마지막에 + 1을 한다.

            var pIterString = pParents.p_mapTest_KeyIsString.GetEnumerator();
            while (pIterString.MoveNext())
                Assert.IsTrue(pIterString.Current.Key == pIterString.Current.Value.name.ToString());

            var pIterEnum = pParents.p_mapTest_KeyIsEnum.GetEnumerator();
            while (pIterEnum.MoveNext())
                Assert.IsTrue(pIterEnum.Current.Key.ToString() == pIterEnum.Current.Value.name.ToString());
        }

        [Test]
        public static void 겟컴포넌트인칠드런은_이름을통해_찾습니다()
        {
            GameObject pObjectParents = new GameObject(nameof(겟컴포넌트인칠드런은_이름을통해_찾습니다));

            // GetComponent 대상인 자식 추가
            for (int i = 0; i < (int) ETestChildObject.MAX; i++)
            {
                GameObject pObjectChild = new GameObject(((ETestChildObject) i).ToString());
                pObjectChild.transform.SetParent(pObjectParents.transform);
                pObjectChild.AddComponent<Test_ComponentChild>();
            }

            GetComponentAttribute_Test pParents = pObjectParents.AddComponent<GetComponentAttribute_Test>();
            Assert.IsNotNull(pParents.p_pChildComponent_FindEnumProperty);
        }

        [Test]
        public static void 겟컴포넌트는_컬렉션에담을수있습니다()
        {
            GameObject pObjectParents = new GameObject(nameof(겟컴포넌트는_컬렉션에담을수있습니다));

            // GetComponent 대상인 자식 추가
            int iAddComponentCount = 3;
            for (int i = 0; i < iAddComponentCount; i++)
                pObjectParents.AddComponent<Test_ComponentOnly>();

            GetComponentAttribute_Test pParents = pObjectParents.AddComponent<GetComponentAttribute_Test>();
            Assert.AreEqual(pParents.arrComponent.Length, iAddComponentCount);
        }

        [Test]
        public static void 겟컴포넌트인칠드런은_같은이름과_같은타입이면_컬렉션에담을수있습니다()
        {
            // Arrange
            GameObject pObjectParents = new GameObject(nameof(겟컴포넌트인칠드런은_같은이름과_같은타입이면_컬렉션에담을수있습니다));
            int iAddComponentCount = 3;
            for (int i = 0; i < iAddComponentCount; i++)
            {
                GameObject pObjectChild = new GameObject("Test");
                pObjectChild.transform.SetParent(pObjectParents.transform);
                pObjectChild.AddComponent<Test_ComponentChild_SameName>();
            }


            // Act
            GetComponentAttribute_Test pParents = pObjectParents.AddComponent<GetComponentAttribute_Test>();



            // Assert
            Assert.AreEqual(pParents.listChildren_NameIs_Test.Count, iAddComponentCount);
        }

        public class GetComponentTestTarget_1 : MonoBehaviour  {  }
        public class GetComponentTestTarget_2 : MonoBehaviour { }

        public class GetComponentTest_GrandParents : MonoBehaviour
        {
            [GetComponentInChildren] public GetComponentTestTarget_1[] arrTest_1 = null;
        }

        public class GetComponentTest_Parents : GetComponentTest_GrandParents
        {
            [GetComponentInChildren] public GetComponentTestTarget_2[] arrTest_2 { get; protected set; }
        }

        public class GetComponentTest_Child : GetComponentTest_Parents
        {
        }

        [Test]
        public static void 조부모클래스의_겟컴포넌트인칠드런_테스트()
        {
            // Arrange (데이터 정렬)
            GetComponentTest_Child pComponentTest = new GameObject(nameof(조부모클래스의_겟컴포넌트인칠드런_테스트)).AddComponent<GetComponentTest_Child>();
            Transform pTransformTest = pComponentTest.transform;

            int iRandomCount_1 = Random.Range(3, 6);
            for(int i = 0; i < iRandomCount_1; i++)
                new GameObject(nameof(GetComponentTestTarget_1)).AddComponent<GetComponentTestTarget_1>().transform.SetParent(pTransformTest);

            int iRandomCount_2 = Random.Range(3, 6);
            for (int i = 0; i < iRandomCount_2; i++)
                new GameObject(nameof(GetComponentTestTarget_2)).AddComponent<GetComponentTestTarget_2>().transform.SetParent(pTransformTest);



            // Act (기능 실행)
            GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(pComponentTest);



            // Arrange (확인)
            Assert.AreEqual(pComponentTest.arrTest_1.Length, iRandomCount_1);
            Assert.AreEqual(pComponentTest.arrTest_2.Length, iRandomCount_2);
        }

        public class GetComponent_ChildrenListTest : MonoBehaviour
        {
            [GetComponentInChildren("Test")]
            public GameObject[] Test_GameObjectArray = null;

            [GetComponentInChildren("Test")]
            public Transform[] Test_TransformArray = null;

            [GetComponentInChildren("Test")]
            public List<Transform> Test_GameObjectList = null;

            [GetComponentInChildren("Test")]
            public List<Transform> Test_TransformList = null;
        }


        [Test]
        public static void 겟컴포넌트인칠드런_리스트_테스트()
        {
            // Arrange (데이터 정렬)
            GetComponent_ChildrenListTest pComponentTest = new GameObject(nameof(겟컴포넌트인칠드런_리스트_테스트)).AddComponent< GetComponent_ChildrenListTest>();
            Transform pTransformTest = pComponentTest.transform;

            int iRandomCount_1 = Random.Range(3, 6);
            for (int i = 0; i < iRandomCount_1; i++)
                new GameObject("Test").transform.SetParent(pTransformTest);

            int iRandomCount_2 = Random.Range(3, 6);
            for (int i = 0; i < iRandomCount_2; i++)
                new GameObject("Test2").transform.SetParent(pTransformTest);



            // Act (기능 실행)
            GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(pComponentTest);



            // Arrange (확인)
            Assert.AreEqual(pComponentTest.Test_GameObjectArray.Length, iRandomCount_1);
            Assert.AreEqual(pComponentTest.Test_TransformArray.Length, iRandomCount_1);
            Assert.AreEqual(pComponentTest.Test_GameObjectList.Count, iRandomCount_1);
            Assert.AreEqual(pComponentTest.Test_TransformList.Count, iRandomCount_1);
        }

        public class GetComponent_ChildrenDictionaryListTest : MonoBehaviour
        {
            public enum EChildObjectName
            {
                Child1,
                Child2,
            }

            [GetComponentInChildren]
            public Dictionary<string, GameObject[]> Test_KeyIs_String_ValueIs_GameObjectArray = new Dictionary<string, GameObject[]>();

            [GetComponentInChildren]
            public Dictionary<string, List<GameObject>> Test_KeyIs_String_ValueIs_GameObjectList = new Dictionary<string, List<GameObject>>();



            [GetComponentInChildren]
            public Dictionary<EChildObjectName, GameObject[]> Test_KeyIs_Enum_ValueIs_GameObjectArray = new Dictionary<EChildObjectName, GameObject[]>();

            [GetComponentInChildren]
            public Dictionary<EChildObjectName, List<GameObject>> Test_KeyIs_Enum_ValueIs_GameObjectList = new Dictionary<EChildObjectName, List<GameObject>>();

        }

        [Test]
        public static void 겟컴포넌트인칠드런_딕셔너리_리스트_테스트()
        {
            // Arrange (데이터 정렬)
            GetComponent_ChildrenDictionaryListTest pComponentTest = new GameObject(nameof(겟컴포넌트인칠드런_딕셔너리_리스트_테스트)).AddComponent<GetComponent_ChildrenDictionaryListTest>();
            Transform pTransformTest = pComponentTest.transform;

            int iRandomCount_1 = Random.Range(3, 6);
            for (int i = 0; i < iRandomCount_1; i++)
                new GameObject(nameof(GetComponent_ChildrenDictionaryListTest.EChildObjectName.Child1)).transform.SetParent(pTransformTest);

            int iRandomCount_2 = Random.Range(3, 6);
            for (int i = 0; i < iRandomCount_2; i++)
                new GameObject(nameof(GetComponent_ChildrenDictionaryListTest.EChildObjectName.Child2)).transform.SetParent(pTransformTest);



            // Act (기능 실행)
            GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(pComponentTest);



            // Arrange (확인)
            Assert.AreEqual(
                pComponentTest.Test_KeyIs_String_ValueIs_GameObjectArray[nameof(GetComponent_ChildrenDictionaryListTest.EChildObjectName.Child1)].Length,
                iRandomCount_1);

            Assert.AreEqual(
                pComponentTest.Test_KeyIs_String_ValueIs_GameObjectArray[nameof(GetComponent_ChildrenDictionaryListTest.EChildObjectName.Child2)].Length,
                iRandomCount_2);
            
            Assert.AreEqual(
                pComponentTest.Test_KeyIs_String_ValueIs_GameObjectList[nameof(GetComponent_ChildrenDictionaryListTest.EChildObjectName.Child1)].Count,
                iRandomCount_1); 
            
            Assert.AreEqual(
                pComponentTest.Test_KeyIs_String_ValueIs_GameObjectList[nameof(GetComponent_ChildrenDictionaryListTest.EChildObjectName.Child2)].Count,
                iRandomCount_2);



            Assert.AreEqual(
                pComponentTest.Test_KeyIs_Enum_ValueIs_GameObjectArray[GetComponent_ChildrenDictionaryListTest.EChildObjectName.Child1].Length,
                iRandomCount_1);

            Assert.AreEqual(
                pComponentTest.Test_KeyIs_Enum_ValueIs_GameObjectArray[GetComponent_ChildrenDictionaryListTest.EChildObjectName.Child2].Length,
                iRandomCount_2);

            Assert.AreEqual(
                pComponentTest.Test_KeyIs_Enum_ValueIs_GameObjectList[GetComponent_ChildrenDictionaryListTest.EChildObjectName.Child1].Count,
                iRandomCount_1);

            Assert.AreEqual(
                pComponentTest.Test_KeyIs_Enum_ValueIs_GameObjectList[GetComponent_ChildrenDictionaryListTest.EChildObjectName.Child2].Count,
                iRandomCount_2);
        }
    }

}