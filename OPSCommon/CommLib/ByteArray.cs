using System;

namespace OPS.Comm.Common.Codify.Celes
{
	/// <summary>
	/// ByteArray is just an array of bytes that auto grows up when needed
	/// </summary>
	internal class ByteArray
	{
		private byte[] _buffer;
		private double _factor;
		/// <summary>
		/// Constructs a new ByteArray with initial size and grow factor of 2
		/// </summary>
		/// <param name="size">Inital size</param>
		internal ByteArray(int size)
		{
			_buffer = new byte[size];
			_factor = 2.0;
		}
		/// <summary>
		/// Constructs a new ByteArray with initial size and grow factor
		/// </summary>
		/// <param name="size">Initial size</param>
		/// <param name="growFactor">Grow Factor (i.e 2.0 ==> array will double size when growing)</param>
		internal ByteArray (int size, double growFactor)
		{
			_buffer = new byte[size];
			if (_factor <= 1.0) 
			{
				throw new ArgumentException ("Grow Factor must be > 1.0", "growFactor");
			}
			_factor = growFactor;
		}

		/// <summary>
		/// Ensures that the ByteArray will have (at least) the number of elements specified
		/// </summary>
		/// <param name="minSize">Minimum capacity to ensure</param>
		internal void EnsureCapacity (int minSize)
		{
			if (_buffer.Length < minSize) GrowUp (minSize);
		}

		/// <summary>
		/// Gets or sets the item at idx position.
		/// If SETTING a position > max index, the array will auto grow up.
		/// If GETTING a position > max index, an exception will be raised.
		/// </summary>
		internal byte this [int idx]
		{
			get 
			{
				return _buffer[idx];
			}
			set
			{
				if (idx > _buffer.Length)
				{
					GrowUp(idx+1);
				}
				_buffer[idx] = value;
			}
		}

		/// <summary>
		/// Gets or sets the grow factor that is used when array must grow up.
		/// </summary>
		internal double GrowFactor
		{
			get { return  _factor; }
			set 
			{
				if (value <= 1.0) 
				{
					throw new ArgumentException ("Grow Factor must be > 1.0", "value");
				}
				_factor = value;

			}
		}

		/// <summary>
		/// Gets the current size of array (# of elements allocated).
		/// </summary>
		internal int Length
		{
			get { return _buffer.Length; }
		}

		/// <summary>
		/// Grows up the array.
		/// </summary>
		/// <param name="minValue">Minimum size that the array must have.</param>
		private void GrowUp(int minValue)
		{
			// try to grow up using factor and check if the new size already contains the minimum size
			int newSize = _buffer.Length + (int)((double)_buffer.Length * _factor);
			// if not, we will grow up until the index.
			if (newSize <minValue) newSize = minValue;
			byte[] tmp = new byte[_buffer.Length];
			Array.Copy (_buffer, 0, tmp, 0, _buffer.Length);
			_buffer = new byte[newSize];
			Array.Copy (tmp, 0, _buffer, 0, tmp.Length);
		}


		/// <summary>
		/// Operator that allows IMPLICIT conversions of ByteArray and byte[].
		/// This is a DANGEROUS operation, because when ByteArray is accessed through a byte[] reference,
		/// no control about size can be done!!!!!
		/// </summary>
		/// <param name="ba">ByteArray to convert (auto called by C# compiler)</param>
		/// <returns>byte[] reference to internal data</returns>
		public static implicit operator byte[] (ByteArray ba)
		{
			return ba._buffer;
		}
	}
}
