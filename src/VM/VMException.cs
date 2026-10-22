using System.Diagnostics.CodeAnalysis;

namespace VM;

[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors", Justification = "VM-ошибки всегда имеют сообщение.")]
public class VmException(string message) : Exception(message);