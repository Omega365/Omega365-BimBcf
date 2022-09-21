using System;
using System.Collections.Generic;
using Xbim.BCF.XMLNodes;
using System.IO.Compression;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Xbim.BCF
{
	public class BCF
	{
		/// <summary>
		/// .bcfp File Representation
		/// </summary>
		public ProjectXMLFile Project { get; set; }
		/// <summary>
		/// .version file Representation
		/// </summary>
		public VersionXMLFile Version { get; set; }
		/// <summary>
		/// A collection of Topics contained within the BCF
		/// </summary>
		public List<Topic> Topics;
        /// <summary>
        /// A collection of IfcProjects contained within the BCF
        /// </summary>
        public List<String> IfcProjects;

        public BCF()
		{
			Topics = new List<Topic>();
		}

		/// <summary>
		/// Creates an object representation of a bcf zip file.
		/// </summary>
		/// <param name="BCFZipData">A Stream of bytes representing a bcf .zip file</param>
		/// <returns>A new BCF object</returns>
		public static BCF Deserialize(Stream BCFZipData)
		{
			BCF bcf = new BCF();
			Topic currentTopic = null;
			Guid currentGuid = Guid.Empty;
			ZipArchive archive = new ZipArchive(BCFZipData);
            Dictionary<Guid, Topic> FileTopics = new Dictionary<Guid, Topic>();
            Dictionary<String, BCFFile> IfcProjectFiles = new Dictionary<String, BCFFile>();
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				if (entry.FullName.EndsWith(".bcfp", StringComparison.OrdinalIgnoreCase))
				{
					bcf.Project = new ProjectXMLFile(XDocument.Load(entry.Open()));
				}
				else if (entry.FullName.EndsWith(".version", StringComparison.OrdinalIgnoreCase))
				{
					bcf.Version = new VersionXMLFile(XDocument.Load(entry.Open()));
				}
				else if (entry.FullName.EndsWith(".bcf", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						if (entry.ExtractGuidFolderName() != currentGuid)
						{
							if (currentTopic != null)
							{
								bcf.Topics.Add(currentTopic);
							}
							currentGuid = entry.ExtractGuidFolderName();
							currentTopic = new Topic();
                            FileTopics.Add(currentGuid, currentTopic);

                        }
						currentTopic.Markup = new MarkupXMLFile(XDocument.Load(entry.Open()));
                        foreach(BCFFile file in currentTopic.Markup.Header.Files)
                        {
                            if (IfcProjectFiles.ContainsKey(file.IfcProject)) { continue; }
                            IfcProjectFiles.Add(file.IfcProject, file);
                        }


					} catch(Exception e)
					{
						Console.WriteLine("Error adding markup", e);
					}
				}
				else if (entry.FullName.EndsWith(".bcfv", StringComparison.OrdinalIgnoreCase))
				{
                    if (entry.ExtractGuidFolderName() != currentGuid && FileTopics.ContainsKey(entry.ExtractGuidFolderName())) {
                        currentGuid = entry.ExtractGuidFolderName();
                        currentTopic = FileTopics[currentGuid];
                    }
                    else if (entry.ExtractGuidFolderName() != currentGuid)
					{
						currentGuid = entry.ExtractGuidFolderName();
						currentTopic = new Topic();
                        FileTopics.Add(currentGuid, currentTopic);
                    }
					try
					{
						currentTopic.Visualization = new VisualizationXMLFile(XDocument.Load(entry.Open()));
					} catch
					{
						Console.WriteLine("Error adding vizualisation");
					}
				}
				else if (entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) 
					|| entry.FullName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
					|| entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
				{
                    if (entry.ExtractGuidFolderName() != currentGuid && FileTopics.ContainsKey(entry.ExtractGuidFolderName()))
                    {
                        currentGuid = entry.ExtractGuidFolderName();
                        currentTopic = FileTopics[currentGuid];
                    }
                    else if(entry.ExtractGuidFolderName() != currentGuid)
					{
						currentGuid = entry.ExtractGuidFolderName();
						currentTopic = new Topic();
                        FileTopics.Add(currentGuid, currentTopic);
                    }
					try
					{
						using (MemoryStream ms = new MemoryStream())
						{
							entry.Open().CopyTo(ms);
							currentTopic.Snapshots.Add(new KeyValuePair<string, byte[]>(entry.FullName, ms.ToArray()));
						}
					} catch
					{
						Console.WriteLine("Error adding snapshot");
					}
				}
			}

			bcf.Topics = new List<Topic>(FileTopics.Values);
            bcf.IfcProjects = new List<string>(IfcProjectFiles.Keys);

			return bcf;
		}

		/// <summary>
		/// Serializes the object to a Stream of bytes representing the bcf .zip file 
		/// </summary>
		/// <returns>A Stream of bytes representing the bcf as a .zip file</returns>
		public Stream Serialize()
		{
			XmlSerializer bcfSerializer = new XmlSerializer(typeof(MarkupXMLFile));
			XmlSerializer bcfvSerializer = new XmlSerializer(typeof(VisualizationXMLFile));
			XmlSerializer bcfpSerializer = new XmlSerializer(typeof(ProjectXMLFile));
			XmlSerializer versionSerializer = new XmlSerializer(typeof(VersionXMLFile));

			var memoryStream = new MemoryStream();
			using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
			{
				if (this.Project != null)
				{
					var bcfp = archive.CreateEntry("project.bcfp");
					using (var bcfpStream = bcfp.Open())
					{
						using (var bcfpWriter = new StreamWriter(bcfpStream))
						{
							bcfpSerializer.Serialize(bcfpWriter, this.Project);
							bcfpWriter.Close();
						}
					}
				}

				if (this.Version != null)
				{
					var version = archive.CreateEntry("bcf.version");
					using (var versionStream = version.Open())
					{
						using (var versionWriter = new StreamWriter(versionStream))
						{
							versionSerializer.Serialize(versionWriter, this.Version);
							versionWriter.Close();
						}
					}
				}

				foreach (Topic t in this.Topics)
				{
					string bcfName = t.Markup.Topic.Guid.ToString() + "/markup.bcf";
					var bcf = archive.CreateEntry(bcfName);
					using (var bcfStream = bcf.Open())
					{
						using (var bcfWriter = new StreamWriter(bcfStream))
						{
							bcfSerializer.Serialize(bcfWriter, t.Markup);
							bcfWriter.Close();
						}
					}

					string bcfvName = t.Markup.Topic.Guid.ToString() + "/viewpoint.bcfv";
					var bcfv = archive.CreateEntry(bcfvName);
					using (var bcfvStream = bcfv.Open())
					{
						using (var bcfvWriter = new StreamWriter(bcfvStream))
						{
							bcfvSerializer.Serialize(bcfvWriter, t.Visualization);
							bcfvWriter.Close();
						}
					}

					foreach (KeyValuePair<String, byte[]> img in t.Snapshots)
					{
						string snapshotName = string.Format("{0}/{1}", t.Markup.Topic.Guid, img.Key);
						var png = archive.CreateEntry(snapshotName);
						using (var pngStream = png.Open())
						{
							using (var pngWriter = new BinaryWriter(pngStream))
							{
								pngWriter.Write(img.Value);
								pngWriter.Close();
							}
						}
					}
				}
			}
			memoryStream.Seek(0, SeekOrigin.Begin);
			return memoryStream;
		}
	}
}
