
# IPTech.Coroutines

A Unity library for coroutines that provides:

Error Handling with Catch / Finally pattern
Faster coroutine iteration
Runs in editor for editor Coroutines

## QuickStart
 - Add the scoped registry to your project
 - Add the depedency to your manifest.json

Packages/manifest.json
```
{
  "scopedRegistries": [
    {
      "name": "IPTech",
      "url": "https://registry.npmjs.com",
      "scopes": [
        "com.iptech"
      ]
    }
  ],
  "dependencies": {
    "com.iptech.coroutines": "1.1.2"
  }
}
```
## Why create this library?
This library was created to add some basic elements that make coroutines much
more programmer friendly.

It aims to treat a coroutine like a void function call, supporting try/catch.

>### Error Handling
>When a Unity coroutine throws an exception, it is simply logged to the console
>and the coroutine fails to update after that. What we need is a way to catch
>that error and respond accordingly. With this library if a coroutine throws an
>exception, it can be caught and handled. It exposes a similar pattern to a
>standard try/catch/finally.

>### Faster coroutine iteration
>It has been proven many times by many engineers online that if you instantiate
>many coroutines in Unity, the default coroutine loop is slow. This is mainly
>due to the internals of how Unity handles the calls to the coroutines. To fix
>this, if you have only 1 update loop that calls many coroutines it is much
>faster.  This library exposes a CoroutineRunner class that does exactly this.

>### Runs editor coroutines
>If you want to use coroutines in the editor, for editor windows or other tasks,
>this library will work by hooking the Editor update mechanism and properly call
>out to your registered coroutines, even in edit mode. There is no special code
>needed to do this, as the CoroutineRunner class will automatically detect the
>mode of operation and ensure your coroutines are updated.

>### Debugging (WIP)
>Debugging coroutines can be difficult, because determining when they are called,
>how long they take, and what they depend on can sometimes be daunting. This
>library provides a visualizer that let's you see the running, completed, and
>dependent coroutines.  It has both an Editor window for use in the editor, and
>a runtime GUI window that you can use in your app so you can see these things
>on device.

## How to use

### The CoroutineFunction
The heart of the library is the CFunc. This class encapsulates the functionality
where the coroutine is iterated. It handles the yield return types, catching
exceptions and delegating those exceptions to handlers when necessary.

You can wrap any IEnumerator with a coroutine function simply by passing it as a
constructor to the object.
```csharp
ICFunc myFunc = new CFunc(DoSomethingCoroutine());
```

Coroutine functions can be updated manually! Yes that's right, if you create a
CFunc, all you have to do is enumerate the object yourself.  It uses the standard
IEnumerator as well... which means you can even yield it to a Unity Coroutine if
you like. This is very useful for unit testing your code that uses coroutines.
But can also be used at runtime as well.

```csharp
[Test]
public void TestMyCoroutine() {
    ICFunc myFunc = new CFunc(DoSomethingCoroutine());

    DateTime timeoutTime = DateTime.Now.AddSeconds(10);
    while(myFunc.MoveNext()) {
        // running coroutine to completion or timeout
        if(DateTime.Now > timeoutTime) {
            Assert.Fail("Test timed out waiting for coroutine to complete.");
            break;
        }
    }

    Assert.IsTrue(myFunc.Error==null);
}
```

You can also check the status of an ICFunc by using the IsDone property.
IsDone will be true when the coroutine completes, with or without error.
```csharp
ICFunc myShuffleCoroutine;

void Start() {
    myShuffleCoroutine = new CFunc(ShuffleCards());
    StartCoroutine(myShuffleCoroutine);
}

void Update() {
    if(myShuffleCoroutine!=null) {
        if(myShuffleCoroutine.IsDone) {
            if(myShuffleCoroutine.Error!=null) {
                // we had an error during shuffle, do some recovery here
            }
            myShuffleCoroutine = null; // clear the
            BeginPlay();
        }
        return;
    }

    //Not shuffling so I can do some other update here
    UpdateAI();
}
```

### Error Handling
Use a new CFunc to register a Catch / Finally action.
Normally, if a coroutine throws an exception, it will cause the coroutine to die,
and any outer coroutine that yielded it will not continue. You can avoid this by
registering a Catch handler, then the error will be swallowed, and any coroutine
that is yielding the coroutine that throws an error will resume as if the yielding
coroutine has finished without error.
Just like a try/catch in a function, you can also throw an exception from the Catch
action and it will fail the coroutine and bubble up as normal.
If and exception is thrown in the Finally block, it will also fail the coroutine
with an Error and bubble up to the containing coroutines.

#### Examples of Catch / Finally
```csharp
// Swallowing an exception silently, allows any outer coroutines to continue
IEnumerator BeginPlayCoroutine() {
    yield return new CFunc(ShuffleCardsCoroutine()).Catch( e => {});
    yield return DealCardsCoroutine(); // <--- will always be called even if ShuffleCardsCoroutine throws an exception
}

// Swallowing an exception with logging, allows any outer coroutines to continue
IEnumerator BeginPlayCoroutine() {
    yield return new CFunc(ShuffleCardsCoroutine()).Catch( e => {
        Debug.LogException(e);
    });
    yield return DealCardsCoroutine(); // <--- will always be called even if ShuffleCardsCoroutine throws an exception
}

// Handling and exception with Catch and Finally
IEnumerator BeginPlayCoroutine() {
    _shuffleDone = false;
    yield return new CFunc(ShuffleCardsCoroutine()).Catch( e => {
        Debug.LogException(e);
        UseFallbackShuffle();
    }).Finally( () => {
        _shuffleDone = true;
    });
    yield return DealCardsCoroutine(); // <--- will always be called even if ShuffleCardsCoroutine throws an exception
}

// Allowing the excpetion to bubble up, but using a Finally block
IEnumerator BeginPlayCoroutine() {
    _shuffleDone = false;
    yield return new CFunc(ShuffleCardsCoroutine()).Finally( () => {
        _shuffleDone = true; // <-- _shuffleDone will be set to true even if the coroutine throws
    });
    yield return DealCardsCoroutine(); // <--- will NOT be called if ShuffleCardsCoroutine throws an exception, but the outer coroutine will bubble the exception.
}
```

### CoroutineRunner
If you want to run your coroutines faster, or even create CFunc coroutines easier
you can use the CoroutineRunner. This class will create a runner that ticks in edit
mode and in play mode. You can create your own instances, and use any patterns you
like for instantiating them.

#### Singleton Pattern
I have provided a singleton "CoroutineUtility". It wraps a call to create the
CoroutineRunner instance. It can be used as in the following.
```csharp

using IPTech.Coroutines;

public class MyExampleBehavior : MonoBehaviour {

    void Start() {
        CoroutineUtility.Start(TestCoroutine());
    }

    public IEnumerator TestCoroutine() {
        int i = 0;
        while (i++ < 10) {
            Debug.Log("I'm in my coroutine for step " + i);
            yield return null;
        }

        Debug.Log("I'm waiting for 5 seconds!");
        yield return new WaitForSeconds(5F);

        Debug.Log("I'm waiting until the end of frame");
        yield return new WaitForEndOfFrame();

        Debug.Log("I'm waiting for fixed update!");
        yield return new WaitForFixedUpdate();

        Debug.Log("I'm waiting for 5 seconds realtime!");
        yield return new WaitForSecondsRealtime(5F);

        Debug.Log("I'm waiting on custom WaitWhile!");
        i = 0;
        yield return new WaitWhile(() => {
            return i++ < 10;
        });

        Debug.Log("I'm waiting on a custom WaitUntil!");
        i = 0;
        yield return new WaitUntil(() => {
            return i++ > 10;
        });

        Debug.Log("I'm waiting on an WWW request!");
        WWW www = new WWW("http://httpbin.org/get");
        yield return www;

        Debug.Log("I'm waiting on an async op!");
        ResourceRequest rr = Resources.LoadAsync<Object>("MyAsyncLoadObject");
        yield return rr;

        Debug.Log("I'm done!");
    }
}

```

#### Custom Instantiation
You can create your own instance, or multiple instances of CoroutineRunner however
you like.  Simply call new CoroutineRunner().

##### Basic Example
```csharp

using IPTech.Coroutines;

// here is my custom singleton in a wrapper class where all my services are declared.
public class MyServices {

    public static ICoroutineRunner CoroutineRunner;

    public void Initialize() {
        coroutineRunner = new CoroutineRunner();    
    }
}

// this class uses the singleton in MyService
public class MyExampleBehaviour : MonoBehaviour {

    void Start() {
        MyServices.CoroutineRunner.Start(TestCoroutine());
    }

    IEnumerator TestCoroutine() {
        yield return null;
        ...
    }
}

// this class uses it's own runner
public class MyExampleWithRunner : MonoBehaviour {
    ICoroutineRunner runner;

    void Start() {
        runner = new CoroutineRunner();
    }

    public void ShuffleTheCards() {
        runner.Start(ShuffleTheCardsCoroutine());
    }

    IEnumerator ShuffleTheCardsCoroutine() {
        // do your shuffle here
    }
}

```

##### Zenject Example
Zenject is a very popular DI framework for Unity, if you like using it, it's simple
to add a CoroutineRunner.

```csharp

public class myInstaller : Installer {
    public override void InstallBindings() {
        Container.Bind<ICoroutineRunner>().To(CoroutineRunner).AsSingle();
    }
}

public class MyMonoBehaviour : MonoBehaviour {

    ICoroutineRunner _coroutineRunner;

    [Inject]
    public void Construct(ICoroutineRunner coroutineRunner) {
        _coroutineRunner = coroutineRunner;
    }

    void Start() {
        _coroutineRunner.Start(TestCoroutine());
    }

    IEnumerator TestCoroutine() {
        yield return null;
        ...
    }
}

```
### Coroutine Debugger
This is a WIP, it is a tool that can visually display the running coroutines and
the waiting on relationships. It will display the current state of the coroutine,
(running, completed, error).

# License

The MIT License (MIT)

Copyright (c) 2019 Ian Pilipski

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
