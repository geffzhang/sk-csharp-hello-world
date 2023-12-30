// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace config;

internal class OpenAIHttpclientHandler : HttpClientHandler
{
    private  KernelSettings _kernelSettings;

    public OpenAIHttpclientHandler(KernelSettings settings)
    {
        this._kernelSettings = settings;
    }


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri.LocalPath == "/v1/chat/completions")
        {
            UriBuilder uriBuilder = new UriBuilder(request.RequestUri)
            {
                Scheme = this._kernelSettings.Scheme,
                Host = this._kernelSettings.Host,
                Port = this._kernelSettings.Port
            };
            request.RequestUri = uriBuilder.Uri;
        }
        return await base.SendAsync(request, cancellationToken);
    }
}

