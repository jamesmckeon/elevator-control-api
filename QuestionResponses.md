### Please describe the differences between IAAS, PAAS and SAAS and give examples of each in a cloud platform of your choosing?

1. IAAS = provider hosts infrastructure, client manages the OS, runtime, middleware,  software, and data (example: a distributed api and database hosted on two Azure VMs)
2. PAAS = provider manages infrastructure, middleware, OS and runtimes; client owns app code, configuration and data (example: Azure Function, Azure SQL database)
3. SAAS = provider manages software and infrastructure, client uses and configures software (eg. O365)

### What are the considerations of a build or buy decision when planning and choosing software?

- Time to market (buy is much quicker usually)
- Available resources (are sufficient developers available to build, or sufficient resources available to train on bought solution)
- Infrastructure: will either require purchase of new infrastructure?  Are resources available to maintain and setup new infrastructure
- Configuration/Integration - can bought solution integrate with enterprise resources if required?  Can bought solution be configured to comply with existing business processes?
- the business becomes tightly coupled to an off the shelf solution, which limits future migration

### What are the foundational elements and considerations when developing a serverless architecture?

- because of their ephemeral lifespans serverless services are difficult to debug. Reproducing the prod environment is oftentimes impossible, and there's no machine to inspect directly.  As a result, observability (logging, distributed tracing across services) must be considered from the ground up.
- idempotency is key, as services will may fail due to consumption limits, throttling or timeouts/cold starts, which creates opportunity for duplicate writes; this situation also requires that serverless solutions implement resiliency/adaptive retry logic
- consumption based pricing is cheap compared to compute for lower traffic functions, but demand/request spikes can cause costs to skyrocket
- serverless services/frameworks promote development of focused, smaller, coordinated and event-driven/asynchronous apis 

### Please describe the concept of composition over inheritance

- a composing class/type uses other types as well as local methods and/or properties to fulfill a contract
- inheritance uses one or more parent types to fulfill a contract (note - not implying here that C# allows for inheriting > 1 parent class, but that parent classes can themselves inherit from other parents)
- composition is usually more flexible and thus less brittle, because it doesnt require coupling to a parent type
- composition also perhaps facilitates implementation of multiple interfaces (the C# contruct, not the OOP concept)

### Describe a design pattern you’ve used in production code. What was the pattern? How did you use it? Given the same problem how would you modify your approach based on your experience?

Recently I came across an implementation of the Observer pattern in an old codebase I had to debug (data wasn't being persisted as expected). It wasn't the right choice because the decoupling that Observer creates between the publisher and subscribers actually made debugging much more difficult, and there wasn't a need in this instance to message multiple consumers at once. If I could do it again I would've simply had the publisher (essentially, a caller) use the subscriber inline, i.e., just call its methods.  This would've been much easier to follow.
