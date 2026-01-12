global using E_Commerce.Core.Models.AuthModels;
global using E_Commerce.Core.Models.CartModels;
global using E_Commerce.Core.Models.ProductModels;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;
global using E_Commerce.Core.Shared.Results;
global using E_Commerce.Core.Shared.Utilties; 
global using Microsoft.AspNetCore.Http;
global using Microsoft.IdentityModel.Tokens;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using E_Commerce.Core.Models.OrderModels;
global using E_Commerce.Repository.Repositories.Interfaces;
global using E_Commerce.Core.Models.TrackingModels;
global using E_Commerce.Repository.Repositories;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Logging;
//global using System.Net.Http.Headers;
global using Microsoft.Extensions.Configuration;
global using MimeKit;
global using MailKit.Net.Smtp;
global using Microsoft.Extensions.Hosting;
global using E_Commerce.Core.Models.InvoiceModels;
global using E_Commerce.Repository.Specifications.InvoiceSpecifications;
global using QuestPDF.Fluent;
global using E_Commerce.Application.Common.DTOS.Requests;
global using E_Commerce.Application.Common.DTOS.Responses;
global using E_Commerce.Application.Common.DTOS.Requests;
global using E_Commerce.Application.Interfaces.Authentication;

global using E_Commerce.Application.Common.Resolvers;
global using E_Commerce.Application.Interfaces.Core;
global using E_Commerce.Core.Shared.Utilties.Enums;
global using System.Globalization;
global using E_Commerce.Core.Shared.Utilties.Identity;
global using Microsoft.AspNetCore.Mvc.Infrastructure;
global using Microsoft.AspNetCore.Mvc.Routing;
global using E_Commerce.Application.Common.DTOS.Responses;




