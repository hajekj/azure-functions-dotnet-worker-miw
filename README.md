This example accompanies a blog post on https://hajekj.net

The purpose is to demonstrate use of Azure AD and Microsoft Graph with Azure Functions Out-of-Process hosting model.

To run this sample, make sure to have user secrets set followingly:

```json
{
    "AzureAd:ClientId": "<client_id>",
    "AzureAd:ClientSecret": "<client_secret>",
    "AzureAd:Instance": "https://login.microsoftonline.com/",
    "AzureAd:TenantId": "<your_tenant_id>"
}
```