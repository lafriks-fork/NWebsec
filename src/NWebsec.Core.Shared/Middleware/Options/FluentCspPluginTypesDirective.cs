﻿// Copyright (c) André N. Klingsheim. See License.txt in the project root for license information.

using System;
using System.Linq;
using NWebsec.Core.Common.HttpHeaders.Configuration;
using NWebsec.Core.Common.HttpHeaders.Configuration.Validation;

namespace NWebsec.Core.Common.Middleware.Options
{
    public class FluentCspPluginTypesDirective : CspPluginTypesDirectiveConfiguration, IFluentCspPluginTypesDirective
    {
        public FluentCspPluginTypesDirective()
        {
            Enabled = true;
        }


#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public void MediaTypes(params string[] mediaTypes)
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        {
            if (mediaTypes == null)
            {
                throw new ArgumentNullException(nameof(mediaTypes));
            }

            if (mediaTypes.Length == 0)
            {
                throw new ArgumentException("One or more parameter values expected.", nameof(mediaTypes));
            }
            var validator = new Rfc2045MediaTypeValidator();
            var types = mediaTypes.Distinct().ToArray();

            foreach (var mediaType in types)
            {
                try
                {
                    validator.Validate(mediaType);
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Invalid argument. Details: " + e.Message, e);
                }
            }

            base.MediaTypes = types;
        }
    }
}