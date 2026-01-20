# .NET Framework Modernisation Case Study
A REST global exception handler and response wrapper for clients consuming an API, originally written in Full .NET Framework, now modernised to .NET 8.0. 
The original, by **Vincent Maverick Durano**, on which it is based, can be found [here](https://github.com/proudmonkey/RESTApiResponseWrapper.Net).
 
This served as an exercise in the modernisation of .NET framework code to .NET, a task likely to become more common as .NET framework's support recedes.
It emulated a common situation of documentation no longer being available, as the previous repo's documentation is now offline.
It also lacked a test suite, so two test cases were added to imitate a proper test suite, which would need to be created for a modernisation project, and would have full coverage and cover all boundary values, needed in a full framework modernisation project.

## Caveat

In its current state this is only suitable for a client-side consumer, where the previous version could cover a server-side API as well. This is because DelegatingHandler (which WrappingHandler inherits from) only interacts with outgoing requests via HttpClient in ASP.NET core and does not take incoming requests. A rewrite toward Middleware, using HttpContext rather than HttpResponseMessage and HttpRequestMessage would be needed to make this work on the server side rather than the client side. Both would be needed to cover the original app's two possible use cases as Middleware relies on an incoming network listener and thus can't be used on the client side.

## Prerequisites

* Microsoft.Extensions.Hosting
* Microsoft.Extensions.Http

## Installing
1) Add the following usings within Program.cs
```
* using Microsoft.Extensions.DependencyInjection;
* using Microsoft.Extensions.Hosting;
* using VMD.RESTApiResponseWrapper.Net;
```

2) Register the following within Program.cs in your host via dependency injection. Use your chosen string key instead of MyExampleAPI, and your API instead of httpbin:
```
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<WrappingHandler>();

        services.AddHttpClient("MyExampleAPI", client =>
        {
            client.BaseAddress = new Uri("https://httpbin.org/");
        })
        .AddHttpMessageHandler<WrappingHandler>();
    })
    .Build();

```

3) You can now create the HttpClient with the string key specified above via the Factory pattern

```
var clientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
var client = clientFactory.CreateClient("MyExampleAPI");

var result = await client.GetStringAsync("get");
Console.WriteLine(result);
Console.ReadLine();
```

## Sample Output 

The following are examples of response output:

Here's the format for successful request with data:

```
{
    
	"Version": "1.0.0.0",
    
	"StatusCode": 200,
    
	"Message": "Request successful.",
    
	"Result": [
		"value1",
        
		"value2"
	]

}
  
```

Here's the format for successful request without data:

```
{
    
	"Version": "1.0.0.0",
    
	"StatusCode": 201,
    
	"Message": "Student with ID 6 has been created."

}
```

Here's the format for error request with validation errors:

```
{
    
	"Version": "1.0.0.0",
    
	"StatusCode": 400,
    
	"Message": "Request responded with exceptions.",
    
	"ResponseException": {
        
		"IsError": true,
        
		"ExceptionMessage": "Validation Field Error.",
        
		"Details": null,
        
		"ReferenceErrorCode": null,
        
		"ReferenceDocumentLink": null,
        
		"ValidationErrors": [
            
			{
                
				"Field": "LastName",
                
				"Message": "'Last Name' should not be empty."
            
			},
            
			{
                
				"Field": "FirstName",
                
				"Message": "'First Name' should not be empty."
            
			}
        ]
    
	}

}
``` 

Here's the format for error request

```
{
    
	"Version": "1.0.0.0",
    
	"StatusCode": 404,
    
	"Message": "Unable to process the request.",
    
	"ResponseException": {
        
		"IsError": true,
        
		"ExceptionMessage": "The specified URI does not exist. Please verify and try again.",

	        "Details": null,
        
		"ReferenceErrorCode": null,
        
		"ReferenceDocumentLink": null,
        
		"ValidationErrors": null
    
	}

} 
```  
          
 

## Using Custom Exception

This library isn't just a custom wrapper, it also provides some objects that you can use for defining your own exception. For example, if you want to throw your own exception message, you could simply do:

```
throw new ApiException("Your Message",401, ModelState.AllErrors());
```

The ApiException has the following parameters that you can set:

```
ApiException(string message,
             int statusCode = 500,
             IEnumerable<ValidationError> errors = null, 
             string errorCode = "", 
             string refLink = "")
```


## Defining Your Own Response Object

Aside from throwing your own custom exception, You could also return your own custom defined Response json by using the ApiResponse object in your API controller. For example:

```
return new APIResponse(201,"Created");
```

The APIResponse has the following parameters:

```
APIResponse(int statusCode, 
	    string message = "", 
	    object result = null, 
            ApiError apiError = null, 
            string apiVersion = "1.0.0.0")
```

## Source Code

The source code for this can be found at https://github.com/themasonator/FrameworkModernisationCaseStudy

## Author

* **Harry Mason**

## Acknowledgements

* **Vincent Maverick Durano** - for creating the project this is based on

* ## License

This project is licensed under the MIT License - see the LICENSE.md file for details
