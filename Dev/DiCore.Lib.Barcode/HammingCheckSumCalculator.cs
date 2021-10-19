namespace DiCore.Lib.Barcode
{
    class HammingCheckSumCalculator:ICheckSumCalculator
    {

        

        public string Calculate(string data)
        {
            var bits = data.GetBits();
            var extendedBits = bits.InsertFakeBits();
            var hammingBitsCount = BitHelper.GetHammingBitsCount((uint) bits.Length);
            var hammingBits = new bool[hammingBitsCount];
            for (var i = 0u; i < hammingBitsCount; i++)
            {
                var mask = BitHelper.GetMask(i, extendedBits.Length);
                var bit = extendedBits.And(mask).XorBits();
                hammingBits[i] = bit;
            }
            return hammingBits.ToHex();
        }
    }
}
