using System.Security.Cryptography;

namespace Checksums;

public class BLAKE3 : HashAlgorithm
{
    private Blake3.Hasher _blake3;
    public BLAKE3()
    {
        InitializeState();
    }

    public new static BLAKE3 Create()
    {
        return new BLAKE3();
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        _blake3.Update(array.AsSpan()[ibStart..cbSize]);
    }

    protected override byte[] HashFinal()
    {
        var hash = _blake3.Finalize();
        return hash.AsSpan().ToArray();
    }

    private void InitializeState()
    {
        Initialize();
    }

    public override void Initialize()
    {
        _blake3 = Blake3.Hasher.New();
    }
}
