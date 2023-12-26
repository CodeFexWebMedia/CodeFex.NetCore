# [CodeFex.NetCore](https://github.com/CodeFexWebMedia/CodeFex.NetCore)

## About
CodeFex.NetCore is a generic common base library for use in Http/Json based applications or in project where Json is the preferred data type.


## Features

* Dynamic **JsonData / JsonDataArray** data type with build-in serialization factories for common data types
* Generic **HttpNetJsonClient** for two data-typed response handling **<Success, Problem>**
* Json based **StagedSettings** for easy section configuraton selection by **Environment.MachineName**
* Based on **System.Text.Json** (Goodbye Newtonsoft.Json) and **System.Net.Http.HttpClient**


## Background

Originally developed for internal usage this source code was refactored out from internal projects and made it to the GitHub.
The project will be used in the comming release of the [CodeFex.Net.Acme](https://github.com/CodeFex.Net.Acme) project.


## Examples

### -> **HttpNetJsonClient/JsonData <Success, Problem>** example
```
// Success: response.Result is not strongly typed!
// Error: response.Error is strongly typed as Problem

var response = await HttpNetJsonClient.Get<JsonData, Problem>(new Uri(BaseUri, ServiceDirectory.MyService), true);

if (response.IsOk)
{
    return response.Result["id"];
}
else
{
    throw new Exception("Operation failed", response.Error.Status);
}
```


### -> **StagedSettings** example

***config.json*** syntax

* [Config:Stage]
    * define key/value pairs: **Environment.MachineName**, **StageName**

* [Config:Source]
    * load nested or secret configuration(s) outside of working directory/source code

```
{
    "[Config:Stage]": {
        "my-workstation": "DEV",
        "my-test-server": "TEST"
    },
    
     "[Config:Source]": {
        "Path": "Z:\\Secrets\\MyService"
    },

    "DEV:MyServiceConfig": {
        "Message": "I'm running in development stage configuration!"
    },

    "TEST:MyServiceConfig": {
        "Message": "I'm running in test stage configuration!"
    },

    "MyServiceConfig": {
        "Message": "I'm running in default configuration!"
    }
}
```

***c#*** code

```
public class MyServiceConfig
{
    public string Message { get; set; }
}
```

```
var stagedSettings = new StagedSettings();
var myServiceConfig = stagedSettings.ReadSection<MyServiceConfig>();

Console.WriteLine(myServiceConfig.Message);

```


## .NET compability
```
net462, net472, net481, netstandard2.0, net5.0, net6.0, net7.0, net8.0
```


## Known project integrations
* **[CodeFex.Net.Acme](https://github.com/CodeFexWebMedia/CodeFex.Net.Acme)** - Acme client library build with **CodeFex.NetCore**


## Acknowledgments

* Portions of this source code is "based on / inspired from" following open source projects

    * [MimeTypeMap](https://github.com/samuelneff/MimeTypeMap) by [samuelneff](https://github.com/samuelneff)

* Portions of this project in build with following tools & resources

    * [jsonlint.com](https://jsonlint.com)
    * [markdownlivepreview.com](https://markdownlivepreview.com)

## Disclaimer

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


## License

This project is licensed under the terms of the [MIT license](https://github.com/CodeFexWebMedia/CodeFex.NetCore/blob/master/LICENSE).

***

**Copyright &copy; 2023** CodeFex Invision AB / CodeFex WebMedia

**Author** Miroslaw Marcinkowski
