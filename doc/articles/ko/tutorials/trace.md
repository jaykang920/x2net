## 로그 메시지 출력

통신 링크를 포함해 x2net의 내부 클래스들은 자체적인 로그 메시지들을 내보낼 수 있지만, 필수적이지 않은 의존성을 줄이고 세상에 널려 있는 더 좋은 (당신 자신의 것을 포함해) 로깅 모듈들을 이용하기 위해 x2net 자체에는 로그를 파일 등으로 출력하는 기능은 포함되어 있지 않습니다.

x2net의 내부 로그 메시지들을 출력하기 위해서는, 단순히 다음과 같이 로그 메시지 핸들러 대리자를 등록해 주기만 하면 됩니다.

```csharp
    public static int Main(string[] args)
    {
        x2net.Trace.Handler = (level, message) => Console.WriteLine(message);

        ...
    }
```

다음과 같이 `x2net.Config.TraceLevel` 속성에 `x2net.TraceLevel` 상수로 지정되는 로그 레벨을 설정해 로그 핸들러로 전달되는 로그 메시지의 수준을 지정할 수도 있습니다. 지정하지 않을 경우의 디폴트 설정은 `TraceLevel.Info` 이상의 로그 레벨만 출력되는 것입니다.

```csharp
    x2net.Config.TraceLevel = x2net.TraceLevel.Debug;
    x2net.Trace.Handler = ...
```