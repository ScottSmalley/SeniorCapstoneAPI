# SeniorCapstoneAPI

FlexPoolAPI:
Formatted to be uploaded as an AWS Lambda function, it checks an HTTPRequest message body for a JSON object. Inside 
the object, there must be an "action" key/value pair with a value corresponding to an action for the API to perform.
Depending on the "action" sent, the API returns either a message of success / failure or data.

API Google doc of API commands: https://docs.google.com/document/d/1FYa0JdBLtg6MJhsW2vNRi18ITzjd0Tyu29xi4rF9sxI/edit?usp=sharing

FlexpoolScheduledEvents:
Formatted to be uploaded as an AWS Lambda function, this group of solutions are intended to be targets for AWS 
CloudWatch to schedule maintenance on our database. These commands are not accessible by a regular user.

To be completed by April 30,2020.
Scott Smalley
UVU Capstone Project 
Fall 2019 - Spring 2020
