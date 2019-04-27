# Unity-GetComponent-Attribute

유니티에서 자주 사용되는 **GetComponent, GetComponentInParents, GetComponentInChildren 등을**

속성으로 구현하여 **원하는 때에 해당 변수나 프로퍼티에 자동으로 할당** 할 수 있는 프로젝트입니다.

![](https://github.com/KorStrix/Unity_GetComponentAttribute/blob/master/Images_ForGhithub/Preview.gif?raw=true)

## 참고한 프로젝트
- https://openlevel.postype.com/post/683269

## 설치 주의사항
유니티 `2017 ~ 2018버젼까지 동작 확인`하였으며,

**유니티 5버젼 이하는 Assembly Definition을 지원하지 않아 정상동작하지 않을 수 있습니다.**

Test 코드로 인하여 ``Assembly Defintion``을 사용했습니다.

## 사용 주의사항

- Awake시 다음과 같이 Manager의 함수를 호출해야 합니다.

```csharp
private void Awake()
{
  // 모노비헤비어를 상속받은 클래스에서 사용하고 싶을 때
    SCManagerGetComponent.DoUpdateGetComponentAttribute(this);
    // 모노비헤비어를 상속받지 않은 클래스에서 사용하고 싶을 때
    SCManagerGetComponent.DoUpdateGetComponentAttribute(this, p_pNotInherit_Mono);
}
```
- 다음과 같이 = null을 안할 경우 유니티 컴파일러가 변수를 할당하지 않았다는 경고를 출력합니다.

```csharp
[GetComponent]
private Transform pTransform; // 컴파일러가 경고 출력

[GetComponent]
private Transform pTransform2 = null; // 컴파일러가 경고를 출력하지 않음
```
## 기능들

#### 1. 유니티가 지원하는 GetComponent, GetComponents, GetComponenInParents, GetComponentInChildren, GetComponentsInChildren 등을 지원

#### 2. GetComponentInChildren Attribute
- 하위 오브젝트에 **같은 타입의 오브젝트가 여러개 있을 경우, 이름으로 찾아서 할당하는 기능**

```csharp

public enum ETestChildObject
{
  TestObject_Other_FindString,
  TestObject_Other_FindEnum,
}

[SerializeField]
[GetComponentInChildren(nameof(ETestChildObject.TestObject_Other_FindString))]
// Attribute 매개변수로 nameof연산자를 통해 string이 들어간 경우입니다.
private Transform p_pChildComponent_FindString = null;

[SerializeField]
[GetComponentInChildren(ETestChildObject.TestObject_Other_FindEnum)]
//
private Transform p_pChildComponent_FindEnum = null;
```

- Array, List, Dictionary 변수 자동 할당 지원 **(Array를 제외한 Collection의 경우 new를 할당해야 합니다.)**

```csharp
[GetComponentInChildren]
public List<Transform> p_listTest = new List<Transform>();

[GetComponentInChildren] // 인자에 Enum을 넣을 경우 오브젝트의 이름을 Enum으로 파싱하여 할당.
private Dictionary<ETestChildObject, Transform> p_mapTest_KeyIsEnum = new Dictionary<ETestChildObject, Transform>();

[GetComponentInChildren] // 인자에 string을 넣을 경우 오브젝트의 이름을 할당.
private Dictionary<string, Transform> p_mapTest_KeyIsString = new Dictionary<string, Transform>();

[SerializeField]
[GetComponentInChildren] // Array의 경우 null을 대입해도 정상 동작
Transform[] arrComponent = null;
```

#### 3. 변수, 프로퍼티 구분없이 지원
- **주의사항으로, set 한정자는 해당 클래스가 접근이 가능하게 해야 합니다.**

```csharp
[GetComponentInChildren]
public Transform p_pChildComponent_FindEnumProperty { get; private set; }
```

#### 4. Monobehaviour를 상속받지 않은 클래스도 지원
```csharp
public class GetComponentAttribute_Example : MonoBehaviour
{
  [System.Serializable] // 인스펙터 노출용
  public class InnerClass_NotInherit_Mono
  {
      [GetComponent]
      public Transform pTransform_Owner;

      [GetComponentInChildren]
      public Transform[] arrTransform_Children;
  }

  public InnerClass_NotInherit_Mono p_pNotInherit_Mono;

  private void Awake()
  {
      // 모노비헤비어를 상속받지 않은 클래스에서 사용하고 싶을 때
      SCManagerGetComponent.DoUpdateGetComponentAttribute(this, p_pNotInherit_Mono);
  }
}
```
#### 5. ChildRequireComponentAttribute
- 자식 오브젝트중에 같은 이름으로 된 해당 타입의 오브젝트가 할당되었는지 인스펙터에 한눈에 파악할 수 있도록 보여줍니다.
  - **Play Mode가 Edit Mode일 경우에도 자동으로 할당하여 체크합니다.**
  - **오딘을 사용할 경우 밑에 따로 한곳에 나열되어 나타납니다.**

- **Before Odin**

![](https://github.com/KorStrix/Unity_GetComponentAttribute/blob/master/Images_ForGhithub/ChildRequireComponent_BeforeOdin.png?raw=true)

- **After Odin** 위와 같은 스크립트지만 프로퍼티 및 Dictionary가 Inspector에 나타납니다.

![](https://github.com/KorStrix/Unity_GetComponentAttribute/blob/master/Images_ForGhithub/ChildRequireComponent_AfterOdin.png?raw=true)


## 연락처
유니티 개발자 모임 카카오톡 & 디스코드 링크입니다.

- 카카오톡 : https://open.kakao.com/o/gOi17az
- 디스코드 : https://discord.gg/9BYFEbG
