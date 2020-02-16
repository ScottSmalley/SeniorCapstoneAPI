/*
 * Utah Valley University
 * CS4550 Capstone Project
 * FlexPool Project
 * Brandon Bezzant
 * Mike Daniel
 * Scott Smalley
 * 
 * Summary:
 * This API uses the Command design pattern
 * to focus on each command or "action" item sent
 * by the user. We take in a request that contains 
 * a body with JSON data. We deserialize it, convert 
 * it to a Dictionary, and determine which type of 
 * command to execute based on that data. We execute 
 * the appropriate command and package it up to be 
 * sent back in a JSON format.
 * 
 * - Designed and developed by Scott Smalley. 
 */
using System;
using Newtonsoft.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Collections.Generic;
using System.Net;
using FlexPoolAPI.Model;

// https://aws.amazon.com/premiumsupport/knowledge-center/build-lambda-deployment-dotnet/ 
// $> dotnet lambda deploy-function --region us-east-2 --function-name flexpooldb
//
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace FlexPoolAPI
{
    public class FlexPoolAPI
    {
        private static readonly JsonSerializer _jsonSerializer = new JsonSerializer();
        //private string mySQLConnectionString;

        /// <summary>
        /// Takes in the APIGatewayProxyRequest, determines
        /// what command is requested, executes the command,
        /// and returns the serialized data in an 
        /// APIGatewayProxyResponse.
        /// </summary>
        /// <param name="apiGatewayRequest"></param>
        /// <returns></returns>
         public APIGatewayProxyResponse Handler(APIGatewayProxyRequest apiGatewayRequest)
        //public APIGatewayProxyResponse Handler(APIGatewayProxyRequest apiGatewayRequest, ILambdaContext lambdaContextnotepad)
        {
            Console.WriteLine(apiGatewayRequest.Path + "\n" + apiGatewayRequest.HttpMethod + "\n" + apiGatewayRequest.Body);

            //Response Object to the HTTP request with default values.
            APIGatewayProxyResponse response = new APIGatewayProxyResponse()
            {
                StatusCode = (int)(HttpStatusCode.NoContent),
                Body = string.Empty
            };

            //New-up Action objects and executes.
            try
            {
                //DB Connection
                //string mySQLConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONN");

                //HTTP request data
                Dictionary<string, string[]> requestBody = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(apiGatewayRequest.Body);

                //Create an Action object which stores the HTTP request data.
                Model.Action newAction = new Model.Action(requestBody, mySQLConnectionString);

                //Object that determines which Command to execute.
                ActionParser parse = new ActionParser();

                //Determine which command to execute, and create the appropriate Command object.
                ActionCommand action = parse.DetermineCommandType(newAction);

                //Object that executes the Command object.
                ActionExecutor executor = new ActionExecutor();

                //Executes the Command, and gets the response data.
                Dictionary<string, string[]> responseData = executor.ExecuteCommand(action);

                //Serializes the data for packaging.
                response.Body = JsonConvert.SerializeObject(responseData);

                response.StatusCode = (int)(HttpStatusCode.OK);
                response.Headers = new Dictionary<string, string> { { "Content-Type", "text/json" } };
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, DELETE");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            }
            catch (ArgumentOutOfRangeException argOutExcep)
            {
                //Timeout exceeded before the response returned.
                Console.WriteLine(argOutExcep.Message);
                Console.WriteLine(argOutExcep.StackTrace);
                response.StatusCode = (int)(HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                //Timeout exceeded before the response returned.
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                response.StatusCode = (int)(HttpStatusCode.InternalServerError);
            }

            Console.WriteLine(response.StatusCode + '\n' + response.Body);
            return response;
        }
    }
}
