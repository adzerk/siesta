#region License
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2009, Enkari, Ltd.
// Licensed under the Apache License, Version 2.0. See the file LICENSE.txt for details.
#endregion
#region Using Directives
using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
#endregion

namespace Siesta
{
    public class ModelSerializationService : IModelSerializationService
    {
	private ILog _logger = LogManager.GetLogger("Siesta.ModelSerializationService");
	public IDictionary<string, IModelSerializer> Serializers { get; private set; }

	public IEnumerable<string> SupportedContentTypes
	{
	    get { return Serializers.Values.Select(s => s.ContentType); }
	}

	public string DefaultContentType
	{
	    get { return Serializers.Values.Where(s => s.IsDefault).Select(s => s.ContentType).Single(); }
	}

	public ModelSerializationService()
	{
             // Hard code to JSON, since that's the only thing we need any more.
	    var serializers = new IModelSerializer[]
	    {
		new Siesta.Serializers.JsonModelSerializer(),
		new Siesta.Serializers.XmlModelSerializer()
	    };
	
	    Serializers = serializers.ToDictionary(plugin => plugin.ContentType);
	}

	public string Serialize(object model, string contentType, ModelFormatting formatting)
	{
	    if (!Serializers.ContainsKey(contentType))
	    throw new NotSupportedException(String.Format("Don't know how to serialize response with MIME type '{0}'", contentType));

	    return Serializers[contentType].Serialize(model, formatting);
	}

	public object Deserialize(Type modelType, string content, string contentType)
	{
	    _logger.Debug("Entering Deserialize");
	
	    if (!Serializers.ContainsKey(contentType)) {
		_logger.DebugFormat("Can't deserialize request of type '{0}'", contentType);
		throw new NotSupportedException(String.Format("Don't know how to deserialize request with MIME type '{0}'", contentType));
	    }

	    _logger.DebugFormat("Deserializing content of type '{0}'", contentType);
	
	    var result = Serializers[contentType].Deserialize(modelType, content);
	    _logger.Debug("Exiting Deserialize");
	    return result;
	}
    }
}
