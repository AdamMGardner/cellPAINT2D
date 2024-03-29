﻿using System.Collections;
using System.Collections.Generic;
using System;

namespace mmtf
{
    /*mmtf.utils.constants.NUM_DICT {1: 'b', 2: '>h', 4: '>i'}*/
    public class Converters
    {
        public static int[] delta_decode(int[] in_array) {
            /*"""A function to delta decode an int array.
           :param in_array: the input array of integers
           :return the decoded array"""
           */
            List<int> out_array = new List<int>();
            if (in_array.Length == 0) { return out_array.ToArray(); }
            int this_ans = in_array[0];
            out_array.Add(this_ans);
            for (int i = 1; i < in_array.Length; i++) {
                this_ans += in_array[i];
                out_array.Add(this_ans);
            }
            return out_array.ToArray();
        }

        //if num is 4 ?
        static byte[] getByteSubArray(byte[] input_byte, int offset, int num)
        {
            byte[] tmp = new byte[num];
            for (int i = 0; i < num; i++)
            {
                tmp[i] = input_byte[offset + num - i - 1];
            }
            return tmp;
        }

        public static short[] convert_bytes_to_ints(byte[] in_bytes, int num)
        {
            /*""Convert a byte array into an integer array. The number of bytes forming an integer
            is defined by num
            :param in_bytes: the input bytes
            :param num: the number of bytes per int
            :return the integer array"""
            */
            short[] out_arr = new short[in_bytes.Length/num];
            for (int i = 0; i < in_bytes.Length/num; i++)
            {
                byte[] val = getByteSubArray(in_bytes, i * num, num);// in_bytes[i * num:i* num +num];//need to reverse as well? between i*num and i*num+num
                out_arr[i] = BitConverter.ToInt16(val, 0);
            }
            return out_arr;
        }

        public static int[] recursive_index_decode(short[] int_array, int max= 32767, int min= -32768) {

            /*"""Unpack an array of integers using recursive indexing.
            :param int_array: the input array of integers
            : param max: the maximum integer size
            :param min: the minimum integer size
            :return the array of integers after recursive index decoding"""
            */

            List<int> out_arr = new List<int>();
            int encoded_ind = 0;

            while (encoded_ind < int_array.Length) {
                int decoded_val = 0;
                while ((int_array[encoded_ind] == max) || (int_array[encoded_ind] == min)) {
                    decoded_val += int_array[encoded_ind];
                    encoded_ind += 1;
                    if (int_array[encoded_ind] == 0)
                        break;
                }
                decoded_val += int_array[encoded_ind];
                encoded_ind += 1;
                out_arr.Add(decoded_val);
             }
            return out_arr.ToArray();
         }

        public static float[] convert_ints_to_floats(int[] in_ints, float divider) {

            /*"""Convert integers to floats by division.
            :param in_ints: the integer array
            :param divider: the divider
            :return the array of floats produced"""
            */
            //divid all int by divider and conver to float
            float[] out_floats = new float[in_ints.Length];
            for (int i = 0; i < in_ints.Length; i++) {
                out_floats[i] = (float)in_ints[i] / divider;
            }
            return out_floats;
        }

        /*
        def convert_ints_to_bytes(in_ints, num):

            """Convert an integer array into a byte arrays. The number of bytes forming an integer

            is defined by num



            :param in_ints: the input integers

            :param num: the number of bytes per int

            :return the integer array"""

            out_bytes= b""

            for val in in_ints:

                out_bytes+=struct.pack(mmtf.utils.constants.NUM_DICT[num], val)

            return out_bytes



        def decode_chain_list(in_bytes):

            """Convert a list of bytes to a list of strings. Each string is of length mmtf.CHAIN_LEN



            :param in_bytes: the input bytes

            :return the decoded list of strings"""

            tot_strings = len(in_bytes) // mmtf.utils.constants.CHAIN_LEN

            out_strings = []

            for i in range(tot_strings):

                out_s = in_bytes[i * mmtf.utils.constants.CHAIN_LEN:i * mmtf.utils.constants.CHAIN_LEN + mmtf.utils.constants.CHAIN_LEN]

                out_strings.append(out_s.decode("ascii").strip(mmtf.utils.constants.NULL_BYTE))

            return out_strings





        def encode_chain_list(in_strings):

            """Convert a list of strings to a list of byte arrays.



            :param in_strings: the input strings

            :return the encoded list of byte arrays"""

            out_bytes = b""

            for in_s in in_strings:

                out_bytes+=in_s.encode('ascii')

                for i in range(mmtf.utils.constants.CHAIN_LEN -len(in_s)):

                    out_bytes+= mmtf.utils.constants.NULL_BYTE.encode('ascii')

            return out_bytes







            def convert_ints_to_chars(in_ints):

            """Convert integers to chars.



            :param in_ints: input integers

            :return the character array converted"""

            return [chr(x) for x in in_ints]



            def convert_floats_to_ints(in_floats, multiplier):

            """Convert floating points to integers using a multiplier.



            :param in_floats: the input floats

            :param multiplier: the multiplier to be used for conversion.Corresponds to the precisison.

            :return the array of integers encoded"""

            return [int(round(x* multiplier)) for x in in_floats]





        def convert_chars_to_ints(in_chars):

            """Convert an array of chars to an array of ints.



            :param in_chars: the input characters

            :return the array of integers"""

            return [ord(x) for x in in_chars]



            def recursive_index_encode(int_array, max= 32767, min= -32768):

            """Pack an integer array using recursive indexing.



            :param int_array: the input array of integers

            :param max: the maximum integer size

            :param min: the minimum integer size

            :return the array of integers after recursive index encoding"""

            out_arr = []

            for curr in int_array:

                if curr >= 0 :

                    while curr >= max:

                        out_arr.append(max)

                        curr -=  max

                else:

                    while curr <= min:

                        out_arr.append(min)

                        curr += int(math.fabs(min))

                out_arr.append(curr)

            return out_arr




        }
        }
        */
    }
}