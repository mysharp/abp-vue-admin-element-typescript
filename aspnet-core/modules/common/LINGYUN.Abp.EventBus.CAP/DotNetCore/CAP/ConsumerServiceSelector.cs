﻿using DotNetCore.CAP.Internal;
using LINGYUN.Abp.EventBus.CAP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;

namespace DotNetCore.CAP
{
    /// <summary>
    /// 消费者查找器
    /// </summary>
    [Dependency(ServiceLifetime.Singleton, ReplaceServices = true)]
    [ExposeServices(typeof(IConsumerServiceSelector), typeof(ConsumerServiceSelector))]

    public class ConsumerServiceSelector : Internal.ConsumerServiceSelector
    {
        /// <summary>
        /// Abp分布式事件配置
        /// </summary>
        protected AbpDistributedEventBusOptions AbpDistributedEventBusOptions { get; }
        /// <summary>
        /// 服务提供者
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Creates a new <see cref="T:DotNetCore.CAP.Internal.ConsumerServiceSelector" />.
        /// </summary>
        public ConsumerServiceSelector(
            IServiceProvider serviceProvider,
            IOptions<AbpDistributedEventBusOptions> distributedEventBusOptions) : base(serviceProvider)
        {
            ServiceProvider = serviceProvider;
            AbpDistributedEventBusOptions = distributedEventBusOptions.Value;
        }
        /// <summary>
        /// 查找消费者集合
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        protected override IEnumerable<ConsumerExecutorDescriptor> FindConsumersFromInterfaceTypes(IServiceProvider provider)
        {
            var executorDescriptorList =
                base.FindConsumersFromInterfaceTypes(provider).ToList();
            //handlers
            var handlers = AbpDistributedEventBusOptions.Handlers;

            foreach (var handler in handlers)
            {
                var interfaces = handler.GetInterfaces();
                foreach (var @interface in interfaces)
                {
                    if (!typeof(IEventHandler).GetTypeInfo().IsAssignableFrom(@interface))
                    {
                        continue;
                    }
                    var genericArgs = @interface.GetGenericArguments();
                    if (genericArgs.Length == 1)
                    {
                        var consumerExecutorDescriptors = GetHandlerDescription(genericArgs[0], handler);

                        foreach (var consumerExecutorDescriptor in consumerExecutorDescriptors)
                        {
                            if (executorDescriptorList.Any(x => new ConsumerExecutorDescriptorComparer().Equals(x, consumerExecutorDescriptor)))
                            {
                                // 如果存在多个消费者,后续的消费者需要重新定义分组才能不被 CAP 框架过滤掉
                                consumerExecutorDescriptor.Attribute.Group = handler.IsGenericType
                                    ? handler.GetGenericTypeDefinition().FullName
                                    : handler.FullName;
                                SetSubscribeAttribute(consumerExecutorDescriptor.Attribute);

                            }
                            executorDescriptorList.Add(consumerExecutorDescriptor);
                        }
                        //Subscribe(genericArgs[0], new IocEventHandlerFactory(ServiceScopeFactory, handler));
                    }
                }
            }
            return executorDescriptorList;
        }
        /// <summary>
        /// 获取事件处理器集合
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        protected virtual IEnumerable<ConsumerExecutorDescriptor> GetHandlerDescription(Type eventType, Type typeInfo)
        {
            var serviceTypeInfo = typeof(IDistributedEventHandler<>)
                .MakeGenericType(eventType);
            var method = typeInfo
                .GetMethod(
                    nameof(IDistributedEventHandler<object>.HandleEventAsync),
                    new[] { eventType }
                );
            // TODO: 事件名称定义在事件参数类型,就无法创建多个订阅者类了,增加可选配置,让用户决定事件名称定义在哪里
            var eventName = EventNameAttribute.GetNameOrDefault(eventType);
            var topicAttr = method.GetCustomAttributes<TopicAttribute>(true);
            var topicAttributes = topicAttr.ToList();

            topicAttributes.Add(new CapSubscribeAttribute(eventName));

            foreach (var attr in topicAttributes)
            {
                SetSubscribeAttribute(attr);

                var parameters = method.GetParameters()
                    .Select(parameter => new ParameterDescriptor
                    {
                        Name = parameter.Name,
                        ParameterType = parameter.ParameterType,
                        IsFromCap = parameter.GetCustomAttributes(typeof(FromCapAttribute)).Any()
                    }).ToList();

                yield return InitDescriptor(attr, method, typeInfo.GetTypeInfo(), serviceTypeInfo.GetTypeInfo(), parameters);
            }
        }

        private static ConsumerExecutorDescriptor InitDescriptor(
            TopicAttribute attr,
            MethodInfo methodInfo,
            TypeInfo implType,
            TypeInfo serviceTypeInfo,
            IList<ParameterDescriptor> parameters)
        {
            var descriptor = new ConsumerExecutorDescriptor
            {
                Attribute = attr,
                MethodInfo = methodInfo,
                ImplTypeInfo = implType,
                ServiceTypeInfo = serviceTypeInfo,
                Parameters = parameters
            };

            return descriptor;
        }
    }
}