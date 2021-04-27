#region License
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2009, Enkari, Ltd.
// Licensed under the Apache License, Version 2.0. See the file LICENSE.txt for details.
#endregion
#region Using Directives
using System;
using System.Web.Mvc;
using Siesta.Framework;
using log4net;
#endregion

namespace Siesta
{
    public class BindRestModelAttribute : CustomModelBinderAttribute
    {
	private ILog _logger = LogManager.GetLogger("Siesta.BindRestModelAttribute");
    
	public override IModelBinder GetBinder()
	{
	    _logger.Debug("Entering GetBinder");
	    return new RestModelBinder();
	}
    }
}
