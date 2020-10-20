![UnityVersion badge](https://img.shields.io/badge/Unity-5.6%2B-red)
![Platform badge](https://img.shields.io/badge/platform-Standalone%20%7C%20Android%20%7C%20IOS-green)
[![License badge](https://img.shields.io/badge/license-MIT-blue)](https://github.com/KorStrix/Unity_GetComponentAttribute/blob/master/LICENSE)

# 개요

유니티에서 자주 사용되는 **GetComponent, GetComponentInParents, GetComponentInChildren 등을**

속성으로 구현하여 **원하는 때에 해당 변수나 프로퍼티에 자동으로 할당** 할 수 있는 프로젝트입니다.

![](https://github.com/KorStrix/Unity_GetComponentAttribute/blob/master/Images_ForGhithub/Preview.gif?raw=true)

- **상기 이미지는 오딘 에셋을 사용한 예시입니다.**
  - 오딘을 사용하지 않을 경우 Property, Dictionary 등이 인스펙터에 나타나지 않습니다.
  - **(Property, Dictionary도 동작은 정상동작합니다. 인스펙터에 그려지지만 않을 뿐입니다.)**

<br>

# 예시

### Before Workflow
```csharp
// 기존 작업 방식은 public이나
public GameObject pLegacyWorkflow_Public_Inspector_Link;

// (private || protected) && SerilizeField로 변경 후 인스펙터에 일일이 드래그 & 드랍방식
[SerializeField]
private GameObject pLegacyWorkflow_Private_Inspector_Link;

[SerializeField]
private GameObject pLegacyWorkflow_Private_InScript;

public Rigidbody pLegacyWorkflow_Property { get; private set; }


private void Awake()
{
  // 혹은 스크립트에 일일이 할당하는 로직
  pLegacyWorkflow_Private_InScript = FindChildObject("Require Object Name");
  pLegacyWorkflow_Property = GetComponentInChildren<Rigidbody>();
}

private GameObject FindChildObject(string strObjectName)
{
  Transform[] arrAllChildObject = GetComponentsInChildren<Transform>();
}
```

### After Workflow

```csharp
[GetComponentInChildren("Somthing Require GameObject Name In Children")]
private GameObject pPrivate_Find_Name;

[GetComponentInChildren]
public Rigidbody pProperty { get; private set; }

void Awake()
{
  // 아래 코드 한줄로 모든 GetComponentAttribute의 필드 혹은 Property가 할당됩니다.
  GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(this);
}
```

<br>

# 사용 주의사항
- Awake시 다음과 같이 Manager의 함수를 호출해야 합니다.

```csharp
private void Awake()
{
    // 모노비헤비어를 상속받은 클래스에서 사용하고 싶을 때
    GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(this);
    // 모노비헤비어를 상속받지 않은 클래스에서 사용하고 싶을 때
    GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(this, p_pNotInherit_Mono);
}
```

- 다음과 같이 = null을 안할 경우 유니티 컴파일러가 변수를 할당하지 않았다는 경고를 출력합니다.

```csharp
[GetComponent]
private Transform pTransform; // 컴파일러가 경고 출력

[GetComponent]
private Transform pTransform2 = null; // 컴파일러가 경고를 출력하지 않음
```

<br>

# 기능들

### 1. 유니티에서 자주 사용하는 함수 GetComponent(s), GetComponenInParents, GetComponent(s)InChildren 등을 지원

### 2. GetComponentInChildren Attribute
- 하위 오브젝트에 **같은 타입의 오브젝트가 여러개 있을 경우, 이름으로 찾아서 할당하는 기능**

```csharp
public enum ETestChildObject
{
  TestObject_Other_FindString,
  TestObject_Other_FindEnum,
}

// Attribute 매개변수로 nameof연산자를 통해 string이 들어간 경우입니다.
[GetComponentInChildren(nameof(ETestChildObject.TestObject_Other_FindString))]
private Transform pChildComponent_FindString = null;

// Attribute 매개변수로 Enum이 들어간 경우입니다.
[GetComponentInChildren(ETestChildObject.TestObject_Other_FindEnum)]
private Transform pChildComponent_FindEnum = null;
```

#### 2-1. Array, List, Dictionary 변수 자동 할당 지원 **(Array를 제외한 Collection의 경우 new를 할당해야 합니다.)**

GameObject의 이름을 기반으로 찾습니다.

```csharp
[GetComponentInChildren]
public List<Transform> listTest = new List<Transform>();

[GetComponentInChildren] // 인자에 Enum을 넣을 경우 오브젝트의 이름을 Enum으로 파싱하여 할당.
private Dictionary<ETestChildObject, Transform> mapTest_KeyIsEnum = new Dictionary<ETestChildObject, Transform>();

[GetComponentInChildren] // 인자에 string을 넣을 경우 오브젝트의 이름을 할당.
private Dictionary<string, Transform> mapTest_KeyIsString = new Dictionary<string, Transform>();

[SerializeField]
[GetComponentInChildren] // Array의 경우 null을 대입해도 정상 동작
Transform[] arrComponent = null;
```

#### 2-2. 중복된 이름의 오브젝트를 Collection로 담는것도 지원 (GetComponent, GetComponentInChidlren).
- Array와 List만 지원합니다.

```csharp
public enum ETestChildObject
{
  TestObject_Other_FindString,
  TestObject_Other_FindEnum,
}

[GetComponent] // 해당 게임오브젝트에 같은 컴포넌트가 있는 경우 여러개가 담김
public List<Transform> listTest = new List<Transform>();

[GetComponentInChildren] // 인자에 Enum을 넣을 경우 오브젝트의 이름을 Enum으로 파싱하여 Enum을 그룹으로 할당.
private Dictionary<ETestChildObject, List<Transform>> mapTest_KeyIsEnum = new Dictionary<ETestChildObject, List<Transform>>();

[GetComponentInChildren] // 인자에 string을 넣을 경우 오브젝트의 이름을 그룹으로 할당.
private Dictionary<string, Transform> mapTest_KeyIsString = new Dictionary<string, Transform>();

```

### 3. 변수, 프로퍼티 구분없이 지원
- **주의사항으로, set 한정자는 해당 클래스가 접근이 가능하게 해야 합니다.**

```csharp
[GetComponentInChildren]
public Transform pChildComponent_FindEnumProperty { get; private set; }
```

### 4. Monobehaviour를 상속받지 않은 클래스도 지원
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
      GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(this, p_pNotInherit_Mono);
  }
}
```

<br>


## 설치 및 사용 방법
1번 혹은 2번 방법 중 하나만 선택하여 설치

### 1. GetComponentAttribute.cs의 내용을 복사하여 설치할 프로젝트에 생성
- 링크 https://github.com/KorStrix/Unity_GetComponentAttribute/blob/master/Runtime/GetComponentAttribute.cs

### 2. Package로 받기 (유니티 2018버전 이상)
- 설치할 유니티 프로젝트 - Packages - manifest.json 파일을 TextEditor로 열어 최하단에 쉼표 및 하단 내용 추가
```
"com.korstrix.getcomponentattribute":"https://github.com/KorStrix/Unity_GetComponentAttribute.git"
```

<br>

## 주의사항

Children 오브젝트가 너무 많을 경우 퍼포먼스 낭비가 심합니다.

<br>

# 그 외

### 참고한 프로젝트
- [Unity3D 자동 GetComponent 블로그 링크](https://openlevel.postype.com/post/683269)

### 연락처
유니티 개발자 모임 카카오톡 & 디스코드 링크입니다.

- 카카오톡 : https://open.kakao.com/o/gOi17az
- 디스코드 : https://discord.gg/9BYFEbG
