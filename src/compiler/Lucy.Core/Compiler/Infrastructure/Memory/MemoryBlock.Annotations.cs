using System;
using System.Collections.Generic;

namespace Disassembler.Infrastructure.Memory
{
	public partial class MemoryBlock
	{
        private readonly List<Annotation> _annotations = new();
        private readonly Dictionary<object, List<uint>> _annotationIndex = new();

		public void AddAnnotation(object key)
		{
			AddAnnotation(Address, key);
		}

		public void AddAnnotation(uint address, object key)
		{
			_annotations.Add(new Annotation(address, key));
			if (_annotationIndex.TryGetValue(key, out var list))
			{
				list.Add(address);
			}
			else
			{
				_annotationIndex.Add(key, new List<uint>() { address });
			}
		}

		public void WriteUInt8(byte value, object annotation)
		{
			AddAnnotation(annotation);
			WriteUInt8(value);
		}

		public void WriteUInt16(ushort value, object annotation)
		{
			AddAnnotation(annotation);
			WriteUInt16(value);
		}

		public void WriteUInt32(uint value, object annotation)
		{
			AddAnnotation(annotation);
			WriteUInt32(value);
		}

		public void WriteUInt64(ulong value, object annotation)
		{
			AddAnnotation(annotation);
			WriteUInt64(value);
		}

		public void WriteUInt8ToAnnotation(object annotation, byte value)
		{
			var org = Address;
			if (!_annotationIndex.TryGetValue(annotation, out var addresses))
			{
				Address = org;
				throw new Exception("Could not find annotation: " + annotation);
			}

			foreach (var address in addresses)
			{
				Address = address;
				WriteUInt8(value);
			}
			Address = org;
		}

		public void WriteUInt16ToAnnotation(object annotation, ushort value)
		{
			var org = Address;
			if (!_annotationIndex.TryGetValue(annotation, out var addresses))
			{
				Address = org;
				throw new Exception("Could not find annotation: " + annotation);
			}

			foreach (var address in addresses)
			{
				Address = address;
				WriteUInt16(value);
			}
			Address = org;
		}

		public void WriteUInt32ToAnnotation(object annotation, uint value)
		{
			var org = Address;
			if (!_annotationIndex.TryGetValue(annotation, out var addresses))
            {
				Address = org;
				throw new Exception("Could not find annotation: " + annotation);
			}

			foreach (var address in addresses)
			{
				Address = address;
				WriteUInt32(value);
			}
			Address = org;
		}

		public void WriteUInt64ToAnnotation(object annotation, ulong value)
		{
			var org = Address;
			if (!_annotationIndex.TryGetValue(annotation, out var addresses))
            {
				Address = org;
				throw new Exception("Could not find annotation: " + annotation);
			}

			foreach (var address in addresses)
			{
				Address = address;
				WriteUInt64(value);
			}
			Address = org;
		}
	}

	public record Annotation(uint Address, object Content);
}
