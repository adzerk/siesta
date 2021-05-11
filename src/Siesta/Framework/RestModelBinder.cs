#region License
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2009, Enkari, Ltd.
// Licensed under the Apache License, Version 2.0. See the file LICENSE.txt for details.
#endregion
#region Using Directives
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;
using Siesta.Serializers;
using Newtonsoft.Json;
#endregion

namespace Siesta.Framework
{
    public class RestModelBinder : IModelBinder
    {
	private ILog _logger = LogManager.GetLogger("Siesta.Framework.RestModelBinder");
        private JsonModelSerializer _jsonSerializer = new JsonModelSerializer();
    
	public IModelSerializationService _serializationService = new ModelSerializationService();
    
	public RestModelBinder()
	{
	}

	public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
	{
	    _logger.Debug("Entering BindModel");
	    var request = controllerContext.HttpContext.Request;
	
	    using (var reader = new StreamReader(request.InputStream))
	    {
		string content = reader.ReadToEnd();
		if (controllerContext.HttpContext.Request.ContentType == "application/x-www-form-urlencoded")
		{
		    _logger.Debug("Content is form encoded");
		    var decoded = HttpUtility.ParseQueryString(content);
		    if (decoded.Count != 1)
		    {
			_logger.ErrorFormat("Ambiguous form encoded content - didn't have exactly one key: {0}",
			    String.Join(", ", decoded.AllKeys));
		        throw new Exception("Ambiguous form encoded content - more than one key");
		    }
		    string decodedContent = decoded[0];
		    _logger.DebugFormat("About to deserialize encoded JSON '{0}'", decodedContent);
		    try
		    {
				var result =_jsonSerializer.Deserialize(bindingContext.ModelType, decodedContent);
				if (result == null)
				{
					throw new HttpException(400, "JSON input deserialization failed.");
				}
				_logger.DebugFormat("Deserialization to type {0}", result == null ? "null" : result.GetType().ToString());
				return result;
		    }
			catch (JsonException)
	        {
				throw new HttpException(400, "Request was not valid JSON.");
			}
		}
		else {
		    string contentType = GetRequestContentType(controllerContext);

	    	    _logger.DebugFormat("content ({1}): {0}", content, contentType);

		    _logger.Debug("Deserializing");
		    var result = _serializationService.Deserialize(bindingContext.ModelType, content, contentType);
		    _logger.Debug("Exiting BindModel");
		    return result;
		}
	    }
	}
    
	private string GetRequestContentType(ControllerContext context)
	{
	    IEnumerable<string> supportedTypes = _serializationService.SupportedContentTypes;
	
	    string[] acceptTypes = context.HttpContext.Request.AcceptTypes;
	    string contentType = context.HttpContext.Request.ContentType;

	    _logger.DebugFormat("Accept types {0}", acceptTypes == null ? "<null>" : String.Join(",", acceptTypes));
	    _logger.DebugFormat("Content type {0}", contentType);
	
	    if (contentType != null)
	    {
		foreach (string type in supportedTypes)
		{
		    if (contentType.Contains(type))
		    {
			_logger.DebugFormat("Determined content type '{0}' from context content type", contentType);
			return type;
		    }
		}
	    }
	
	    if (acceptTypes != null)
	    {
		foreach (string type in supportedTypes)
		{
		    if (acceptTypes.Contains(type))
		    {
			_logger.DebugFormat("Determined content type '{0}' from accepted types", type);
			return type;
		    }
		}
	    }

	    _logger.DebugFormat("Using default content type '{0}'", _serializationService.DefaultContentType);
	
	    return _serializationService.DefaultContentType;
	}
    }
}
