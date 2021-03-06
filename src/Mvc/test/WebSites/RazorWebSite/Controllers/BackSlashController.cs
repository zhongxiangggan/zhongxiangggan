// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc;

namespace RazorWebSite.Controllers
{
    public class BackSlashController : Controller
    {
        public IActionResult Index() => View(@"Views\BackSlash\BackSlashView.cshtml");
    }
}