using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;

namespace OpenNoxLibrary.Util
{
    // If you want to have a read:
    // http://www.cs.columbia.edu/~hgs/audio/dvi/IMA_ADPCM.pdf
    // https://wiki.multimedia.cx/index.php/Westwood_IMA_ADPCM (not an actual format used there)
    public static class ADPCMDecoder
    {
        static readonly int[] index_table = 
        { 
            -1, -1, -1, -1, 2, 4, 6, 8
        };
        static readonly int[] step_table =
        {
            7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
            19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
            50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
            130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
            337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
            876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
            2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
            5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
            15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
        };

        public static unsafe byte[] EXPAND_PCM_MONO_TO_STEREO(byte[] input)
        {
            byte[] result = new byte[input.Length * 2];

            fixed (byte* ptr = result)
            {
                for (int i = 0; i < input.Length; i+=2)
                {
                    ptr[i * 2] = input[i];
                    ptr[i * 2 + 1] = input[i + 1];
                    ptr[i * 2 + 2] = input[i];
                    ptr[i * 2 + 3] = input[i + 1];
                }
            }
            return result;
        }

        /// <summary> 
        /// Decompresses 4-bit DVI ADPCM encoded audio data (prefixed with 4-byte header) into raw 16-bit signed LSB sample array.
        /// </summary>
        public static unsafe byte[] DVI_ADPCM_TO_RAW_PCM(byte[] input)
        {
            var inputLength = input.Length;

            // DVI4 ADPCM header: https://www.freesoft.org/CIE/RFC/1890/10.htm 
            int sample = (int)(input[0] | (input[1] << 8));
            int index = input[2];
            // The remaining byte is 'reserved'

            inputLength -= 4;

            var outputLength = inputLength * 4; // 4 bits expanded to 16 bits
            var output = new byte[outputLength];
            var sample_count = inputLength * 2;

            fixed (byte* outb = output)
            {
                short* outbuf = (short*)outb;

                int code;
	            int delta;
	            int step;

	            for (int sample_index = 0; sample_index < sample_count; sample_index++)
	            {
		            code = input[4 + (sample_index >> 1)];
		            code = (sample_index & 1) > 0 ? code >> 4 : code & 0xf;
		            step = step_table[index];
		            delta = step >> 3;
		            if ((code & 1) > 0)
			            delta += step >> 2;
		            if ((code & 2) > 0)
			            delta += step >> 1;
		            if ((code & 4) > 0)
			            delta += step;
		            if ((code & 8) > 0)
		            {
			            sample -= delta;
			            if (sample < -32768)
				            sample = -32768;
		            }
		            else
		            {
			            sample += delta;
			            if (sample > 32767)
				            sample = 32767;
		            }
		            outbuf[sample_index] = (short) sample;
		            index += index_table[code & 7];
		            if (index < 0)
			            index = 0;
		            else if (index > 88)
			            index = 88;  
	            }
            }

            return output;
        }
    }
}
