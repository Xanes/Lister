using Microsoft.OpenApi.Models;
using PDFReader.DTOs;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using Domain.Models;
using System.Text.Json;
using Microsoft.OpenApi.Any;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PDFReader.SwaggerConfig
{
    /// <summary>
    /// Adds examples to Swagger documentation
    /// </summary>
    public class SwaggerExampleFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Only add examples for specific operations
            if (context.MethodInfo.Name == "AddAdditionalProducts")
            {
                // Create example for AddAdditionalProducts endpoint
                var example = new
                {
                    shoppingListId = 1,
                    categoryProducts = new List<object>
                    {
                        new
                        {
                            categoryId = 1,
                            products = new List<object>
                            {
                                new
                                {
                                    name = "Apple",
                                    quantity = 3.0,
                                    quantityUnit = "pcs",
                                    weight = (double?)null,
                                    weightUnit = (string?)null,
                                    isChecked = false
                                },
                                new
                                {
                                    name = "Banana",
                                    quantity = 1.0,
                                    quantityUnit = "kg",
                                    weight = (double?)null,
                                    weightUnit = (string?)null,
                                    isChecked = false
                                }
                            }
                        },
                        new
                        {
                            categoryId = 2,
                            products = new List<object>
                            {
                                new
                                {
                                    name = "Chicken",
                                    quantity = (double?)null,
                                    quantityUnit = (string?)null,
                                    weight = 500.0,
                                    weightUnit = "g",
                                    isChecked = false
                                }
                            }
                        }
                    }
                };

                // Add the example to the request body
                var requestContent = operation.RequestBody.Content.FirstOrDefault(x => x.Key == "application/json").Value;
                
                if (requestContent != null && requestContent.Examples == null)
                {
                    // Use JsonConvert and JObject for better conversion
                    var json = JsonConvert.SerializeObject(example);
                    var jobj = JObject.Parse(json);
                    
                    requestContent.Examples = new Dictionary<string, OpenApiExample>
                    {
                        {
                            "AdditionalProductsExample",
                            new OpenApiExample
                            {
                                Summary = "Example products to add to shopping list",
                                Description = "This example shows adding three products (Apple, Banana, and Chicken) to a shopping list with ID 1. It demonstrates both quantity and weight-based products.",
                                Value = jobj.ToOpenApiAny()
                            }
                        }
                    };
                }
            }
        }
    }

    // Extension method to convert JToken to OpenApiAny
    public static class JTokenExtensions
    {
        public static IOpenApiAny ToOpenApiAny(this JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    var obj = new OpenApiObject();
                    foreach (var property in token.Children<JProperty>())
                    {
                        obj.Add(property.Name, property.Value.ToOpenApiAny());
                    }
                    return obj;

                case JTokenType.Array:
                    var arr = new OpenApiArray();
                    foreach (var item in token.Children())
                    {
                        arr.Add(item.ToOpenApiAny());
                    }
                    return arr;

                case JTokenType.Integer:
                    return new OpenApiInteger(token.Value<int>());

                case JTokenType.Float:
                    return new OpenApiDouble(token.Value<double>());

                case JTokenType.String:
                    return new OpenApiString(token.Value<string>());

                case JTokenType.Boolean:
                    return new OpenApiBoolean(token.Value<bool>());

                case JTokenType.Null:
                    return new OpenApiNull();

                default:
                    return new OpenApiString(token.ToString());
            }
        }
    }
} 