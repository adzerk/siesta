#region License
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2009, Enkari, Ltd.
// Licensed under the Apache License, Version 2.0. See the file LICENSE.txt for details.
#endregion
#region Using Directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
#endregion

namespace Siesta
{
    public class ModelResult<T> : ActionResult
    {
	private IModelSerializationService _serializationService;

	public T Model { get; private set; }
	public ModelFormatting Formatting { get; private set; }

	public ModelResult(T model)
	    : this(model, ModelFormatting.Normal, new ModelSerializationService())
	{
	}

	public ModelResult(T model, ModelFormatting formatting)
	    : this(model, formatting,  new ModelSerializationService())
	{
	}

	public ModelResult(T model, ModelFormatting formatting, IModelSerializationService serializationService)
	{
	    Model = model;
	    Formatting = formatting;
	    _serializationService = serializationService;
	}

	public override void ExecuteResult(ControllerContext context)
	{
	    HttpResponseBase response = context.HttpContext.Response;

	    response.ContentEncoding = Encoding.UTF8;
	    response.ContentType = GetResponseContentType(context);

	    response.Write(_serializationService.Serialize(Model, response.ContentType, Formatting));
	}

	private string GetResponseContentType(ControllerContext context)
	{
	    IEnumerable<string> supportedTypes = _serializationService.SupportedContentTypes;

	    string[] acceptTypes = context.HttpContext.Request.AcceptTypes;
	    string contentType = context.HttpContext.Request.ContentType;

	    if (acceptTypes != null)
	    {
		foreach (string type in supportedTypes)
		{
		    if (acceptTypes.Contains(type))
		    return type;
		}
	    }

	    if (contentType != null)
	    {
		foreach (string type in supportedTypes)
		{
		    if (contentType.Contains(type))
		    return type;
		}
	    }

	    return _serializationService.DefaultContentType;
	}
    }
}
