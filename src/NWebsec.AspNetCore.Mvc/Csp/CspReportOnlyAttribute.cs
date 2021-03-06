﻿// Copyright (c) André N. Klingsheim. See License.txt in the project root for license information.

using System;
using NWebsec.AspNetCore.Mvc.Csp.Internals;

namespace NWebsec.AspNetCore.Mvc.Csp
{
    /// <summary>
    /// When applied to a controller or action method, enables the Content-Security-Policy-Report-Only header (assuming there are directives enabled). 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public class CspReportOnlyAttribute : CspAttributeBase
    {
        protected override bool ReportOnly => true;
    }
}
