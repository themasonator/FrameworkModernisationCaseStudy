# .NET Framework Modernisation Case Study
A REST global exception handler and response wrapper for Web API originally written in Full .NET Framework, now modernised to .NET 8.0.
The original, by **Vincent Michael Durano** on which it is based, can be found [here](https://github.com/proudmonkey/RESTApiResponseWrapper.Net).

This served as a case study for modernisation of .NET framework code to .NET, a task likely to become more common as .NET framework's support recedes.
In addition it emulated a common situation of documentation no longer being available as the previous repo's documentation is now offline.
It also lacked a test suite, so two test cases were added to imitate a proper test suite, which would need to be created for a modernisation project, and would have full coverage and cover all boundary values, needed in a full framework modernisation project.

## Prerequisites

* ASP.NET core 8+
* Newtonsoft.Json

## Installing

1) Declare the following namespace within Program.cs

using VMD.RESTApiResponseWrapper.Net;
using VMD.RESTApiResponseWrapper.Net.Filters;

2) Register the following within Program.cs

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ApiExceptionFilter>();
    })
    .AddNewtonsoftJson();
builder.Services.AddTransient<WrappingHandler>();
app.MapControllers();

3) Done.

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

* **Vincent Michael Durano** - for creating the project this is based on

* ## License

This project is licensed under the MIT License - see the LICENSE.md file for details
