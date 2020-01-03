/*
 * Utah Valley University
 * CS4400 Capstone Project
 * FlexPool Project
 * Brandon Bezzant
 * Mike Daniel
 * Joseph Dominguez-Virla
 * Scott Smalley
 * 
 * Summary:
 * This API uses the Command design pattern
 * to focus on each command or "action" item sent
 * by the user. It uses loose coupling and 
 * high cohesion to create a modular framework
 * to make it easy to create/modify commands without
 * modifying the main code execution.
 * We take in a request that contains a body with JSON data.
 * We deserialize it, convert it to a Dictionary, and
 * determine which type of command to execute based on that
 * data. We execute the appropriate command and package it
 * up to be sent back in a JSON format.
 * Developed by Scott Smalley
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
        private string mySQLConnectionString;

        //All our execution happens in here. We need to build out inside this Handler method. 
        //It's what gets called by AWS Lambda.
        public APIGatewayProxyResponse Handler(APIGatewayProxyRequest apiGatewayRequest)
        {
            Console.WriteLine(apiGatewayRequest.Path + "\n" + apiGatewayRequest.HttpMethod + "\n" + apiGatewayRequest.Body);

            //Acts as our return from Lambda to the user. Later on we 
            //use the response.Body property to Serialize a JSON object to act as the
            //data going back to the user.
            APIGatewayProxyResponse response = new APIGatewayProxyResponse()
            {
                StatusCode = (int)(HttpStatusCode.NoContent),
                Body = string.Empty
            };

            try
            {
                Console.WriteLine("apiGatewayRequest:  " + apiGatewayRequest);
                mySQLConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONN");

                //The data that we need to use is in the request Body.
                Dictionary<string, string[]> requestBody = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(apiGatewayRequest.Body);

                //Create an Action object which stores the Dictionary 
                //created from the request body and the DB connection string.
                Model.Action newAction = new Model.Action(requestBody, mySQLConnectionString);

                //Determine which Command object to use for execution.
                ActionParser parse = new ActionParser();

                //Creates the appropriate subclass of ActionCommand to
                //execute next.
                ActionCommand action = parse.DetermineCommandType(newAction);

                //Executor Object.
                ActionExecutor executor = new ActionExecutor();

                //Takes the result of executing the ActionCommand into a Dictionary
                //to be sent back to the user.
                Dictionary<string, string[]> responseData = executor.ExecuteCommand(action);

                //Serializes the result to be sent back.
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
