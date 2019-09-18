
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
namespace CustomInputManager
{
	public class InputSaverXML 
	{
		
		private string m_filename;
		
		public InputSaverXML(string filename)
		{
			if(filename == null)
				throw new ArgumentNullException("filename");
			
			m_filename = filename;
		}

		private XmlWriter CreateXmlWriter(XmlWriterSettings settings)
		{
			if(m_filename != null)
			{
// #if UNITY_WINRT && !UNITY_EDITOR
// 				m_outputStream = new MemoryStream();
// 				return XmlWriter.Create(m_outputStream, settings);
// #else
				return XmlWriter.Create(m_filename, settings);
// #endif
			}
			
			return null;
		}

		public void Save(List<ControlScheme> controlSchemes)
		{
			if (controlSchemes == null)
				return;
				
			XmlWriterSettings xmlSettings = new XmlWriterSettings();
			xmlSettings.Encoding = Encoding.UTF8;
			xmlSettings.Indent = true;

			using(XmlWriter writer = CreateXmlWriter(xmlSettings))
			{
				writer.WriteStartDocument(true);
				writer.WriteStartElement("Input");
				
				foreach(ControlScheme scheme in controlSchemes)
				{
					WriteControlScheme(scheme, writer);
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();
			}

// #if UNITY_WINRT && !UNITY_EDITOR
// 			if(m_filename != null && m_outputStream != null && (m_outputStream is MemoryStream))
// 			{
// 				UnityEngine.Windows.File.WriteAllBytes(m_filename, ((MemoryStream)m_outputStream).ToArray());
// 				m_outputStream.Dispose();
// 			}
// #endif
		}

		private void WriteControlScheme(ControlScheme scheme, XmlWriter writer)
		{
			writer.WriteStartElement("ControlScheme");
			writer.WriteAttributeString("name", scheme.Name);
			// writer.WriteAttributeString("id", scheme.UniqueID);
			foreach(var action in scheme.Actions)
			{
				WriteInputAction(action, writer);
			}

			writer.WriteEndElement();
		}

		private void WriteInputAction(InputAction action, XmlWriter writer)
		{
			writer.WriteStartElement("Action");
			writer.WriteAttributeString("name", action.Name);
			writer.WriteAttributeString("displayName", action.displayName);
			
			foreach(var binding in action.bindings)
			{
				WriteInputBinding(binding, writer);
			}

			writer.WriteEndElement();
		}

		private void WriteInputBinding(InputBinding binding, XmlWriter writer)
		{
			writer.WriteStartElement("Binding");
			writer.WriteElementString("Positive", binding.Positive.ToString());
			writer.WriteElementString("Negative", binding.Negative.ToString());

			writer.WriteElementString("DeadZone", binding.DeadZone.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("Gravity", binding.Gravity.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("Sensitivity", binding.Sensitivity.ToString(CultureInfo.InvariantCulture));


			writer.WriteElementString("Snap", binding.SnapWhenReadAsAxis.ToString().ToLower());
			writer.WriteElementString("Invert", binding.InvertWhenReadAsAxis.ToString().ToLower());


			writer.WriteElementString("UseNeg", binding.useNegativeAxisForButton.ToString());
			writer.WriteElementString("Rebindable", binding.rebindable.ToString());
			writer.WriteElementString("SensitivityEditable", binding.sensitivityEditable.ToString());
			writer.WriteElementString("InvertEditable", binding.invertEditable.ToString());

	
			writer.WriteElementString("Type", binding.Type.ToString());
			writer.WriteElementString("Axis", binding.MouseAxis.ToString());

			writer.WriteElementString("GamepadButton", binding.GamepadButton.ToString());
			writer.WriteElementString("GamepadAxis", binding.GamepadAxis.ToString());
			
			writer.WriteEndElement();

		}
	}
}
