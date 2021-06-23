// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace MvcSandbox.Controllers
{
    public class HomeController : Controller
    {
        [ModelBinder]
        public string Id { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/foo")]
        public IActionResult Foo()
        {
            var type = typeof(ControllerBase).Assembly.GetType("Microsoft.AspNetCore.Mvc.HotReload.HotReloadService");
            var method = type.GetMethod("ClearCache", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            method.Invoke(null, new object[] { null });

            return Ok();
        }
    }
}
