public class PRNG
{
    private int seed;

	public PRNG(int seed)
	{
		this.seed = seed;
	}

	public int Next(int max)
	{
		// "& 0x7fffffff" makes the int always positive by making the sign bit 0
		seed = (seed * 1103515245 + 12345) & 0x7fffffff;
		return seed % max;
	}
}