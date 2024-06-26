﻿using IPB.LogicApp.Standard.Testing.Model;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunOverview;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace IPB.LogicApp.Standard.Testing.Local.Helpers
{
	public class WorkflowHelper
	{

		public string WorkflowName { get; set; }

		public ManagementApiHelper ManagementApiHelper { get; set; }



		/// <summary>
		/// Gets the callback url for the logic app so we can trigger it.  The default behaviour is to get the manual trigger so we can run over
		/// http
		/// </summary>
		/// <param name="triggerName"></param>
		/// <returns></returns>
		public string GetCallBackUrl(string triggerName = "manual")
		{
			var url = string.Format(
				"{0}/{1}/triggers/{2}/listCallbackUrl?api-version={3}",
				ApiSettings.ManagementWorkflowBaseUrl,
				WorkflowName,
				triggerName,
				ApiSettings.ApiVersion);

			var client = ManagementApiHelper.GetHttpClient();

			var content = new StringContent("");
			HttpResponseMessage response = client.PostAsync(url, content).Result;
			var responseText = response.Content.ReadAsStringAsync().Result;
			response.EnsureSuccessStatusCode();

			var jsonResponse = JObject.Parse(responseText);
			return jsonResponse["value"].ToString();
		}

		/// <summary>
		/// Triggers the logic app with an HTTP post request
		/// </summary>
		/// <param name="content"></param>
		/// <param name="triggerName"></param>
		/// <returns></returns>
		public WorkFlowResponse TriggerLogicAppWithPost(HttpContent content, string triggerName = "manual", Dictionary<string, string> queryParameters = null)
		{
			var url = GetCallBackUrl(triggerName);

			if (queryParameters != null)
			{
				url = string.Join("&", url, queryParameters.Select(q => $"{q.Key}={q.Value}"));
			}

			using (HttpClient client = new HttpClient())
			{
				HttpResponseMessage response = client.PostAsync(url, content).Result;
				var workflowResponse = new WorkFlowResponse(response);
				return workflowResponse;
			}
		}

		/// <summary>
		/// Triggers the logic app with an HTTP GET request
		/// </summary>
		/// <param name="content"></param>
		/// <param name="triggerName"></param>
		/// <returns></returns>
		public WorkFlowResponse TriggerLogicAppWithGet(StringContent content, string triggerName = "manual", Dictionary<string, string> queryParameters = null)
		{
			var url = GetCallBackUrl(triggerName);

			if (queryParameters != null)
			{
				url = string.Join("&", url, queryParameters.Select(q => $"{q.Key}={q.Value}"));
			}

			using (HttpClient client = new HttpClient())
			{
				HttpResponseMessage response = client.GetAsync(url).Result;
				var workflowResponse = new WorkFlowResponse(response);
				return workflowResponse;
			}
		}

		/// <summary>
		/// Once have ran the logic app we can get a workflow run helper which will let us access details about the run history
		/// </summary>
		/// <param name="runId"></param>
		/// <returns></returns>
		public WorkflowRunHelper GetWorkflowRunHelper(string runId)
		{
			var runHelper = new WorkflowRunHelper();
			runHelper.WorkflowHelper = this;
			runHelper.RunId = runId;
			runHelper.ManagementApiHelper = ManagementApiHelper;
			return runHelper;
		}

		public RunDetails GetMostRecentRunDetails(DateTime startDate)
		{
			var dateString = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");

			var url = $@"{ApiSettings.ManagementWorkflowBaseUrl}/{WorkflowName}/runs?api-version={ApiSettings.ApiVersion}&$filter=startTime ge {dateString}";

			var client = ManagementApiHelper.GetHttpClient();

			Console.WriteLine($"Query for recent workflow run");
			Console.WriteLine($"{url}");
			HttpResponseMessage response = client.GetAsync(url).Result;
			var responseText = response.Content.ReadAsStringAsync().Result;
			response.EnsureSuccessStatusCode();

			var runList = JsonConvert.DeserializeObject<WorkflowRunList>(responseText);

			return runList.Value.FirstOrDefault();
		}

		public RunDetails GetMostRecentRun()
		{
			var url = $@"{ApiSettings.ManagementWorkflowBaseUrl}/{WorkflowName}/runs?api-version={ApiSettings.ApiVersion}&$top=1";

			var client = ManagementApiHelper.GetHttpClient();

			Console.WriteLine($"Query for recent workflow run");
			Console.WriteLine($"{url}");
			HttpResponseMessage response = client.GetAsync(url).Result;
			var responseText = response.Content.ReadAsStringAsync().Result;
			response.EnsureSuccessStatusCode();

			var runList = JsonConvert.DeserializeObject<WorkflowRunList>(responseText);

			return runList.Value.FirstOrDefault();
		}

		public WorkflowRunList GetRunsSince(DateTime startDate)
		{
			var dateString = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
			var url = $@"{ApiSettings.ManagementWorkflowBaseUrl}/{WorkflowName}/runs?api-version={ApiSettings.ApiVersion}&$filter=startTime ge {dateString}";

			var client = ManagementApiHelper.GetHttpClient();

			Console.WriteLine($"Query for workflow runs since");
			Console.WriteLine($"{url}");
			HttpResponseMessage response = client.GetAsync(url).Result;
			var responseText = response.Content.ReadAsStringAsync().Result;
			response.EnsureSuccessStatusCode();

			return JsonConvert.DeserializeObject<WorkflowRunList>(responseText);
		}
	}
}
