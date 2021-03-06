using System.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{
    public MethodReference GetLoggerMethod;

    public void FindGetLoggerMethod()
    {
        var loggerFactoryAttribute = ModuleDefinition
            .Assembly
            .CustomAttributes
            .FirstOrDefault(x => x.AttributeType.Name == "LoggerFactoryAttribute");

        if (loggerFactoryAttribute == null)
        {
            LogInfo("Could not find a 'LoggerFactoryAttribute' on the current assembly. Going to search current assembly for 'LoggerFactory'.");

            var typeDefinition = ModuleDefinition
                .GetTypes()
                .FirstOrDefault(x => !x.IsGenericInstance && x.Name == "LoggerFactory");
            if (typeDefinition == null)
            {
                throw new WeavingException("Could not find a type named LoggerFactory");
            }

            FindGetLogger(typeDefinition);
        }
        else
        {
            var typeReference = (TypeReference) loggerFactoryAttribute.ConstructorArguments.First().Value;

            FindGetLogger(typeReference.Resolve());
            GetLoggerMethod = ModuleDefinition.Import(GetLoggerMethod);
        }

    }

    void FindGetLogger(TypeDefinition typeDefinition)
    {
        GetLoggerMethod = typeDefinition
            .Methods
            .FirstOrDefault(x =>
                x.Name ==  "GetLogger" && 
                x.IsStatic &&
                x.HasGenericParameters &&
                x.GenericParameters.Count == 1 &&
                x.Parameters.Count == 0);

        if (GetLoggerMethod == null)
        {
            throw new WeavingException("Found 'LoggerFactory' but it did not have a static 'GetLogger' method that takes 'string' as a parameter");
        }
    }
}