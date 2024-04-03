﻿using IPB.LogicApp.Standard.Testing.Local.Helpers;
using IPB.LogicApp.Standard.Testing.Model;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunActionDetails;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunOverview;
using Microsoft.Extensions.Configuration.UserSecrets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace IPB.LogicApp.Standard.Testing.Local
{
	public class LogicAppTestManager
	{
		private ManagementApiHelper _managementApiHelper;
		private WorkflowHelper _workflowHelper;
		private LogicAppTestManagerArgs _args;
		private WorkFlowResponse _workflowResponse;
		private WorkflowRunHelper _workflowRunHelper;

		public LogicAppTestManager(LogicAppTestManagerArgs args)
		{
			_args = args;
		}

		public void Setup()
		{
			//Setup Management API Helper
			_managementApiHelper = new ManagementApiHelper();

			//Setup Workflow Helper
			_workflowHelper = new WorkflowHelper();
			_workflowHelper.WorkflowName = _args.WorkflowName;
			_workflowHelper.ManagementApiHelper = _managementApiHelper;
		}

		/// <summary>
		/// Trigger the logic app with an HTTP post
		/// </summary>
		/// <param name="content"></param>
		/// <param name="triggerName"></param>
		/// <returns></returns>
		public WorkFlowResponse TriggerLogicAppWithPost(HttpContent content, string triggerName = "manual", Dictionary<string, string> queryParameters = null)
		{
			_workflowResponse = _workflowHelper.TriggerLogicAppWithPost(content, triggerName, queryParameters);
			return _workflowResponse;
		}

		/// <summary>
		/// Trigger the logic app with an HTTP get
		/// </summary>
		/// <param name="content"></param>
		/// <param name="triggerName"></param>
		/// <returns></returns>
		public WorkFlowResponse TriggerLogicAppWithGet(StringContent content, string triggerName = "manual", Dictionary<string, string> queryParameters = null)
		{
			_workflowResponse = _workflowHelper.TriggerLogicAppWithGet(content, triggerName, queryParameters);
			return _workflowResponse;
		}


		/// <summary>
		/// Loads the run history of the executed logic app.  This needs to be called before you start testing the actions
		/// that got executed
		/// </summary>
		public void LoadWorkflowRunHistory()
		{
			if (_workflowResponse == null)
				throw new Exception("You havent triggered the logic app");

			_workflowRunHelper = _workflowHelper.GetWorkflowRunHelper(_workflowResponse.WorkFlowRunId);
			_workflowRunHelper.GetRunActions();
			_workflowRunHelper.GetRunDetails();
		}

		public void LoadWorkflowRunHistory(string runId)
		{
			_workflowRunHelper = _workflowHelper.GetWorkflowRunHelper(runId);
			_workflowRunHelper.GetRunActions();
			_workflowRunHelper.GetRunDetails();
		}

		public WorkflowRunList GetRunsSince(DateTime startDate)
		{
			return _workflowHelper.GetRunsSince(startDate);
		}

		public RunDetails GetMostRecentRunSince(DateTime startDate)
		{
			return _workflowHelper.GetMostRecentRunDetails(startDate);
		}

		public RunDetails GetMostRecentRun()
		{
			return _workflowHelper.GetMostRecentRun();
		}

		public string GetMostRecentRunIdSince(DateTime startDate)
		{
			return _workflowHelper.GetMostRecentRunDetails(startDate).id;
		}

		/// <summary>
		/// Get the overall status of the workfow
		/// </summary>
		/// <param name="refresh"></param>
		/// <returns></returns>
		public WorkflowRunStatus GetWorkflowRunStatus(bool refresh = false)
		{
			var runDetails = _workflowRunHelper.GetRunDetails(refresh);
			return runDetails.properties.WorkflowRunStatus;
		}

		/// <summary>
		/// Get the action from the run history so you can check if it was successful
		/// </summary>
		/// <param name="actionName"></param>
		/// <param name="refreshActions"></param>
		/// <param name="formatActionName"></param>
		/// <returns></returns>
		public ActionStatus GetActionStatus(string actionName, bool refreshActions = false, bool formatActionName = true)
		{
			return _workflowRunHelper.GetActionStatus(actionName, refreshActions, formatActionName);
		}

		/// <summary>
		/// Get the input message to an action
		/// </summary>
		/// <param name="actionName"></param>
		/// <param name="refreshActions"></param>
		/// <param name="formatActionName"></param>
		/// <returns></returns>
		public string GetActionInputMessage(string actionName, bool refreshActions = false, bool formatActionName = true)
		{
			var action = _workflowRunHelper.GetActionJson(actionName, refreshActions, formatActionName);
			var url = action["inputsLink"]?["uri"]?.Value<string>();
			var httpClient = new HttpClient();
			var response = httpClient.GetAsync(url).Result;
			response.EnsureSuccessStatusCode();
			return response.Content.ReadAsStringAsync().Result;
		}

		/// <summary>
		/// Get the input message to an action
		/// </summary>
		/// <param name="actionName"></param>
		/// <param name="refreshActions"></param>
		/// <param name="formatActionName"></param>
		/// <returns></returns>
		public string GetActionOutputMessage(string actionName, bool refreshActions = false, bool formatActionName = true)
		{
			var action = _workflowRunHelper.GetActionJson(actionName, refreshActions, formatActionName);
			var url = action["outputsLink"]?["uri"]?.Value<string>();
			var httpClient = new HttpClient();
			var response = httpClient.GetAsync(url).Result;
			response.EnsureSuccessStatusCode();
			return response.Content.ReadAsStringAsync().Result;
		}

		/// <summary>
		/// Get the action json if you want to inspect it within your test
		/// </summary>
		/// <param name="actionName"></param>
		/// <param name="refreshActions"></param>
		/// <param name="formatActionName"></param>
		/// <returns></returns>
		public JToken GetActionJson(string actionName, bool refreshActions = false, bool formatActionName = true)
		{
			return _workflowRunHelper.GetActionJson(actionName, refreshActions, formatActionName);
		}

		/// <summary>
		/// Get the trigger result so you can check its status
		/// </summary>
		/// <param name="refresh"></param>
		/// <returns></returns>
		public TriggerStatus GetTriggerStatus(bool refresh = false)
		{
			return _workflowRunHelper.GetTriggerStatus(refresh);
		}
	}
}
