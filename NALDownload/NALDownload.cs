using NALDownload.Instructions;

namespace NALDownload;
public static class NALDownload
{
    public static async Task<IEnumerable<Instruction>> GenerateUpdateInstructions(string oldVersionDirectoryPath, string newVersionDirectoryPath, Action<string>? onProgressUpdate = null)
    {
        if (!(Directory.Exists(oldVersionDirectoryPath) && Directory.Exists(newVersionDirectoryPath)))
            throw new ArgumentException("Both paths must point into an existing directory");

        List<Instruction> instructions = new();

        // Start enumeration from root
        await foreach (Instruction i in InstructionGenerator.EnumerateGeneratedDirectoryInstructionsAsync(oldVersionDirectoryPath, newVersionDirectoryPath, string.Empty, onProgressUpdate))
            instructions.Add(i);

        return instructions;
    }

    // UNTESTED
    // UNTESTED
    // UNTESTED
    // UNTESTED
    // UNTESTED
    // UNTESTED
    // UNTESTED
    // UNTESTED
    public static async Task<IEnumerable<Instruction>> GenerateVerificationInstructions(string directoryPath, Action<string>? onProgressUpdate = null)
    {
        if (!Directory.Exists(directoryPath))
            throw new ArgumentException("Path must point into an existing directory");

        List<Instruction> instructions = new();

        // Start enumeration from root
        await foreach (Instruction i in VerificationGenerator.EnumerateGeneratedDirectoryVerificationsAsync(directoryPath, string.Empty, onProgressUpdate))
            instructions.Add(i);

        return instructions;
    }

    public static async Task ApplyUpdateInstructionsAsync(string oldVersionDirectoryPath, IEnumerable<Instruction> instructions, Action<string>? onProgressUpdate = null)
    {
        await InstructionHandler.ApplyInstructionsOnDirectory(oldVersionDirectoryPath, instructions, onProgressUpdate);
    }
}
