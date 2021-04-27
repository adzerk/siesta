#region License
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2009, Enkari, Ltd.
// Licensed under the Apache License, Version 2.0. See the file LICENSE.txt for details.
#endregion
#region Using Directives
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Siesta.Framework;
using log4net;
#endregion

namespace Siesta.Serializers
{
    public class XmlModelSerializer : IModelSerializer
    {
	private ILog _logger = LogManager.GetLogger("Siesta.Serializers.XmlModelSerializer");
	private readonly Dictionary<RuntimeTypeHandle, DataContractSerializer> _serializers = new Dictionary<RuntimeTypeHandle, DataContractSerializer>();

	public string ContentType
	{
	    get { return "application/xml"; }
	}

	public bool IsDefault
	{
	    get { return true; }
	}

	public string Serialize(object model, ModelFormatting formatting)
	{
	    using (var buffer = new MemoryStream(1024))
	    using (var writer = new ModelXmlWriter(buffer, Encoding.UTF8, formatting))
	    {
		DataContractSerializer serializer = GetSerializerForType(model.GetType());

		writer.WriteStartDocument();
		serializer.WriteObject(writer, model);
		writer.Flush();

		return Encoding.UTF8.GetString(buffer.ToArray());
	    }
	}

	public object Deserialize(Type modelType, string content)
	{
	    _logger.Debug("Entering Deserialize");
	    DataContractSerializer serializer = GetSerializerForType(modelType);

	    _logger.Debug("Got serializer");
	    using (var stream = new StringReader(content)) {
		_logger.Debug("Got stream");
		try {
		    using (var reader = new ModelXmlReader(stream))
		    {
			_logger.Debug("About to deserialize");
			object result;
			try {
			    result = serializer.ReadObject(reader);
			}
			catch (Exception x)
			{
			    _logger.Error("Failed to deserialize", x);
			    throw;
			}
			_logger.Debug("Exiting Deserialize");
			return result;
		    }
		}
		catch (Exception x)
		{
		    _logger.Error("Could not get ModelXmlReader", x);
		    throw;
		}
	    }
	}

	private DataContractSerializer GetSerializerForType(Type type)
	{
	    _logger.Debug("Entering GetSerializerForType");
	    RuntimeTypeHandle handle = type.TypeHandle;

	    if (_serializers.ContainsKey(handle)) {
		_logger.Debug("Exiting GetSerializerForType (a)");
		return _serializers[handle];
	    }

	    var serializer = new DataContractSerializer(type);
	    _serializers.Add(handle, serializer);

	    _logger.Debug("Exiting GetSerializerForType (b)");
	
	    return serializer;
	}
    }
}
