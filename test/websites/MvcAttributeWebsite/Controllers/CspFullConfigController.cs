﻿// Copyright (c) André N. Klingsheim. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace MvcAttributeWebsite.Controllers
{
    public class CspFullConfigController : Controller
    {
        //
        // GET: /CspConfig/

        public ActionResult Index()
        {
            return View("Index");
        }
    }
}
