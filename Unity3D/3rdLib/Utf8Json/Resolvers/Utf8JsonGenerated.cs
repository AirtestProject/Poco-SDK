using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace Utf8Json.Resolvers
{
    using System;
    using Utf8Json;

    public class GeneratedResolver : global::Utf8Json.IJsonFormatterResolver
    {
        public static readonly global::Utf8Json.IJsonFormatterResolver Instance = new GeneratedResolver();

        GeneratedResolver()
        {

        }

        public global::Utf8Json.IJsonFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::Utf8Json.IJsonFormatter<T> formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::Utf8Json.IJsonFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(26)
            {
                {typeof(global::System.Collections.Generic.Dictionary<Transform, global::Game.SDKs.PocoSDK.NodeInfo>), 0 },
                {typeof(global::System.Collections.Generic.List<global::Game.SDKs.PocoSDK.NodeInfo>), 1 },
                {typeof(global::System.Collections.Generic.List<string>), 2 },
                {typeof(global::System.Collections.Generic.List<global::Poco.AbstractNode>), 3 },
                {typeof(global::System.Collections.Generic.Dictionary<string, object>), 4 },
                {typeof(object[]), 5 },
                {typeof(global::Utf8Json.IJsonFormatter[]), 6 },
                {typeof(global::Utf8Json.IJsonFormatterResolver[]), 7 },
                {typeof(global::System.Collections.Generic.List<Transform>), 8 },
                {typeof(global::Game.SDKs.PocoSDK.NodeParams), 9 },
                {typeof(global::Game.SDKs.PocoSDK.NodeInfo), 10 },
                {typeof(global::CustomDumper), 11 },
                {typeof(global::PerfDataManager), 12 },
                {typeof(global::PocoManager), 13 },
                {typeof(global::TcpServer.TcpClientState), 14 },
                {typeof(global::DelayTaskObj), 15 },
                {typeof(global::TcpServer.TcpClientConnectedEventArgs), 16 },
                {typeof(global::TcpServer.TcpClientDisconnectedEventArgs), 17 },
                {typeof(global::Poco.AbstractNode), 18 },
                {typeof(global::Poco.RootNode), 19 },
                {typeof(global::Utf8Json.JsonFormatterAttribute), 20 },
                {typeof(global::Utf8Json.SerializationConstructorAttribute), 21 },
                {typeof(global::Utf8Json.FormatterNotRegisteredException), 22 },
                {typeof(global::Utf8Json.JsonParsingException), 23 },
                {typeof(global::Utf8Json.Resolvers.DynamicCompositeResolver), 24 },
                {typeof(global::Poco.UnityNode), 25 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new global::Utf8Json.Formatters.DictionaryFormatter<Transform, global::Game.SDKs.PocoSDK.NodeInfo>();
                case 1: return new global::Utf8Json.Formatters.ListFormatter<global::Game.SDKs.PocoSDK.NodeInfo>();
                case 2: return new global::Utf8Json.Formatters.ListFormatter<string>();
                case 3: return new global::Utf8Json.Formatters.ListFormatter<global::Poco.AbstractNode>();
                case 4: return new global::Utf8Json.Formatters.DictionaryFormatter<string, object>();
                case 5: return new global::Utf8Json.Formatters.ArrayFormatter<object>();
                case 6: return new global::Utf8Json.Formatters.ArrayFormatter<global::Utf8Json.IJsonFormatter>();
                case 7: return new global::Utf8Json.Formatters.ArrayFormatter<global::Utf8Json.IJsonFormatterResolver>();
                case 8: return new global::Utf8Json.Formatters.ListFormatter<Transform>();
                case 9: return new Utf8Json.Formatters.Game.SDKs.PocoSDK.NodeParamsFormatter();
                case 10: return new Utf8Json.Formatters.Game.SDKs.PocoSDK.NodeInfoFormatter();
                case 11: return new Utf8Json.Formatters.CustomDumperFormatter();
                case 12: return new Utf8Json.Formatters.PerfDataManagerFormatter();
                case 13: return new Utf8Json.Formatters.PocoManagerFormatter();
                case 14: return new Utf8Json.Formatters.TcpServer.TcpClientStateFormatter();
                case 15: return new Utf8Json.Formatters.DelayTaskObjFormatter();
                case 16: return new Utf8Json.Formatters.TcpServer.TcpClientConnectedEventArgsFormatter();
                case 17: return new Utf8Json.Formatters.TcpServer.TcpClientDisconnectedEventArgsFormatter();
                case 18: return new Utf8Json.Formatters.Poco.AbstractNodeFormatter();
                case 19: return new Utf8Json.Formatters.Poco.RootNodeFormatter();
                case 20: return new Utf8Json.Formatters.Utf8Json.JsonFormatterAttributeFormatter();
                case 21: return new Utf8Json.Formatters.Utf8Json.SerializationConstructorAttributeFormatter();
                case 22: return new Utf8Json.Formatters.Utf8Json.FormatterNotRegisteredExceptionFormatter();
                case 23: return new Utf8Json.Formatters.Utf8Json.JsonParsingExceptionFormatter();
                case 24: return new Utf8Json.Formatters.Utf8Json.Resolvers.DynamicCompositeResolverFormatter();
                case 25: return new Utf8Json.Formatters.Poco.UnityNodeFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning disable 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Utf8Json.Formatters.Game.SDKs.PocoSDK
{
    using System;
    using Utf8Json;


    public sealed class NodeParamsFormatter : global::Utf8Json.IJsonFormatter<global::Game.SDKs.PocoSDK.NodeParams>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public NodeParamsFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("transform"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("name"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("gameObject"), 2},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("image"), 3},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("text"), 4},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("collider"), 5},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("button"), 6},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("rawImage"), 7},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("rectTransform"), 8},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("spriteRenderer"), 9},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("layer"), 10},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("canvas"), 11},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("rootCanvas"), 12},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("rootCanvasRectTransform"), 13},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("type"), 14},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("transform"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("name"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("gameObject"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("image"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("text"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("collider"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("button"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("rawImage"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("rectTransform"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("spriteRenderer"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("layer"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("canvas"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("rootCanvas"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("rootCanvasRectTransform"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("type"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::Game.SDKs.PocoSDK.NodeParams value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<Transform>().Serialize(ref writer, value.transform, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            writer.WriteString(value.name);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<GameObject>().Serialize(ref writer, value.gameObject, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[3]);
            formatterResolver.GetFormatterWithVerify<Image>().Serialize(ref writer, value.image, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[4]);
            formatterResolver.GetFormatterWithVerify<Text>().Serialize(ref writer, value.text, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[5]);
            formatterResolver.GetFormatterWithVerify<Collider>().Serialize(ref writer, value.collider, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[6]);
            formatterResolver.GetFormatterWithVerify<Button>().Serialize(ref writer, value.button, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[7]);
            formatterResolver.GetFormatterWithVerify<RawImage>().Serialize(ref writer, value.rawImage, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[8]);
            formatterResolver.GetFormatterWithVerify<RectTransform>().Serialize(ref writer, value.rectTransform, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[9]);
            formatterResolver.GetFormatterWithVerify<SpriteRenderer>().Serialize(ref writer, value.spriteRenderer, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[10]);
            writer.WriteInt32(value.layer);
            writer.WriteRaw(this.____stringByteKeys[11]);
            formatterResolver.GetFormatterWithVerify<Canvas>().Serialize(ref writer, value.canvas, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[12]);
            formatterResolver.GetFormatterWithVerify<Canvas>().Serialize(ref writer, value.rootCanvas, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[13]);
            formatterResolver.GetFormatterWithVerify<RectTransform>().Serialize(ref writer, value.rootCanvasRectTransform, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[14]);
            writer.WriteString(value.type);
            
            writer.WriteEndObject();
        }

        public global::Game.SDKs.PocoSDK.NodeParams Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __transform__ = default(Transform);
            var __transform__b__ = false;
            var __name__ = default(string);
            var __name__b__ = false;
            var __gameObject__ = default(GameObject);
            var __gameObject__b__ = false;
            var __image__ = default(Image);
            var __image__b__ = false;
            var __text__ = default(Text);
            var __text__b__ = false;
            var __collider__ = default(Collider);
            var __collider__b__ = false;
            var __button__ = default(Button);
            var __button__b__ = false;
            var __rawImage__ = default(RawImage);
            var __rawImage__b__ = false;
            var __rectTransform__ = default(RectTransform);
            var __rectTransform__b__ = false;
            var __spriteRenderer__ = default(SpriteRenderer);
            var __spriteRenderer__b__ = false;
            var __layer__ = default(int);
            var __layer__b__ = false;
            var __canvas__ = default(Canvas);
            var __canvas__b__ = false;
            var __rootCanvas__ = default(Canvas);
            var __rootCanvas__b__ = false;
            var __rootCanvasRectTransform__ = default(RectTransform);
            var __rootCanvasRectTransform__b__ = false;
            var __type__ = default(string);
            var __type__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __transform__ = formatterResolver.GetFormatterWithVerify<Transform>().Deserialize(ref reader, formatterResolver);
                        __transform__b__ = true;
                        break;
                    case 1:
                        __name__ = reader.ReadString();
                        __name__b__ = true;
                        break;
                    case 2:
                        __gameObject__ = formatterResolver.GetFormatterWithVerify<GameObject>().Deserialize(ref reader, formatterResolver);
                        __gameObject__b__ = true;
                        break;
                    case 3:
                        __image__ = formatterResolver.GetFormatterWithVerify<Image>().Deserialize(ref reader, formatterResolver);
                        __image__b__ = true;
                        break;
                    case 4:
                        __text__ = formatterResolver.GetFormatterWithVerify<Text>().Deserialize(ref reader, formatterResolver);
                        __text__b__ = true;
                        break;
                    case 5:
                        __collider__ = formatterResolver.GetFormatterWithVerify<Collider>().Deserialize(ref reader, formatterResolver);
                        __collider__b__ = true;
                        break;
                    case 6:
                        __button__ = formatterResolver.GetFormatterWithVerify<Button>().Deserialize(ref reader, formatterResolver);
                        __button__b__ = true;
                        break;
                    case 7:
                        __rawImage__ = formatterResolver.GetFormatterWithVerify<RawImage>().Deserialize(ref reader, formatterResolver);
                        __rawImage__b__ = true;
                        break;
                    case 8:
                        __rectTransform__ = formatterResolver.GetFormatterWithVerify<RectTransform>().Deserialize(ref reader, formatterResolver);
                        __rectTransform__b__ = true;
                        break;
                    case 9:
                        __spriteRenderer__ = formatterResolver.GetFormatterWithVerify<SpriteRenderer>().Deserialize(ref reader, formatterResolver);
                        __spriteRenderer__b__ = true;
                        break;
                    case 10:
                        __layer__ = reader.ReadInt32();
                        __layer__b__ = true;
                        break;
                    case 11:
                        __canvas__ = formatterResolver.GetFormatterWithVerify<Canvas>().Deserialize(ref reader, formatterResolver);
                        __canvas__b__ = true;
                        break;
                    case 12:
                        __rootCanvas__ = formatterResolver.GetFormatterWithVerify<Canvas>().Deserialize(ref reader, formatterResolver);
                        __rootCanvas__b__ = true;
                        break;
                    case 13:
                        __rootCanvasRectTransform__ = formatterResolver.GetFormatterWithVerify<RectTransform>().Deserialize(ref reader, formatterResolver);
                        __rootCanvasRectTransform__b__ = true;
                        break;
                    case 14:
                        __type__ = reader.ReadString();
                        __type__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::Game.SDKs.PocoSDK.NodeParams();
            if(__transform__b__) ____result.transform = __transform__;
            if(__name__b__) ____result.name = __name__;
            if(__gameObject__b__) ____result.gameObject = __gameObject__;
            if(__image__b__) ____result.image = __image__;
            if(__text__b__) ____result.text = __text__;
            if(__collider__b__) ____result.collider = __collider__;
            if(__button__b__) ____result.button = __button__;
            if(__rawImage__b__) ____result.rawImage = __rawImage__;
            if(__rectTransform__b__) ____result.rectTransform = __rectTransform__;
            if(__spriteRenderer__b__) ____result.spriteRenderer = __spriteRenderer__;
            if(__layer__b__) ____result.layer = __layer__;
            if(__canvas__b__) ____result.canvas = __canvas__;
            if(__rootCanvas__b__) ____result.rootCanvas = __rootCanvas__;
            if(__rootCanvasRectTransform__b__) ____result.rootCanvasRectTransform = __rootCanvasRectTransform__;
            if(__type__b__) ____result.type = __type__;

            return ____result;
        }
    }


    public sealed class NodeInfoFormatter : global::Utf8Json.IJsonFormatter<global::Game.SDKs.PocoSDK.NodeInfo>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public NodeInfoFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("param"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("parent"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("childTransforms"), 2},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("param"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("parent"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("childTransforms"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::Game.SDKs.PocoSDK.NodeInfo value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<global::Game.SDKs.PocoSDK.NodeParams>().Serialize(ref writer, value.param, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<Transform>().Serialize(ref writer, value.parent, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<HashSet<global::Game.SDKs.PocoSDK.NodeInfo>>().Serialize(ref writer, value.childTransforms, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::Game.SDKs.PocoSDK.NodeInfo Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __param__ = default(global::Game.SDKs.PocoSDK.NodeParams);
            var __param__b__ = false;
            var __parent__ = default(Transform);
            var __parent__b__ = false;
            var __childTransforms__ = default(HashSet<global::Game.SDKs.PocoSDK.NodeInfo>);
            var __childTransforms__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __param__ = formatterResolver.GetFormatterWithVerify<global::Game.SDKs.PocoSDK.NodeParams>().Deserialize(ref reader, formatterResolver);
                        __param__b__ = true;
                        break;
                    case 1:
                        __parent__ = formatterResolver.GetFormatterWithVerify<Transform>().Deserialize(ref reader, formatterResolver);
                        __parent__b__ = true;
                        break;
                    case 2:
                        __childTransforms__ = formatterResolver.GetFormatterWithVerify<HashSet<global::Game.SDKs.PocoSDK.NodeInfo>>().Deserialize(ref reader, formatterResolver);
                        __childTransforms__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::Game.SDKs.PocoSDK.NodeInfo();
            if(__param__b__) ____result.param = __param__;
            if(__parent__b__) ____result.parent = __parent__;
            if(__childTransforms__b__) ____result.childTransforms = __childTransforms__;

            return ____result;
        }
    }

}

#pragma warning disable 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Utf8Json.Formatters
{
    using System;
    using Utf8Json;


    public sealed class CustomDumperFormatter : global::Utf8Json.IJsonFormatter<global::CustomDumper>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public CustomDumperFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("trmTree"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("list"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("rQueue"), 2},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("canvasList"), 3},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("trmTree"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("list"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("rQueue"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("canvasList"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::CustomDumper value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<Transform, global::Game.SDKs.PocoSDK.NodeInfo>>().Serialize(ref writer, value.trmTree, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::Game.SDKs.PocoSDK.NodeInfo>>().Serialize(ref writer, value.list, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Concurrent.ConcurrentQueue<global::System.Collections.Generic.Dictionary<string, object>>>().Serialize(ref writer, value.rQueue, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[3]);
            formatterResolver.GetFormatterWithVerify<HashSet<global::Game.SDKs.PocoSDK.NodeParams>>().Serialize(ref writer, value.canvasList, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::CustomDumper Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __trmTree__ = default(global::System.Collections.Generic.Dictionary<Transform, global::Game.SDKs.PocoSDK.NodeInfo>);
            var __trmTree__b__ = false;
            var __list__ = default(global::System.Collections.Generic.List<global::Game.SDKs.PocoSDK.NodeInfo>);
            var __list__b__ = false;
            var __rQueue__ = default(global::System.Collections.Concurrent.ConcurrentQueue<global::System.Collections.Generic.Dictionary<string, object>>);
            var __rQueue__b__ = false;
            var __canvasList__ = default(HashSet<global::Game.SDKs.PocoSDK.NodeParams>);
            var __canvasList__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __trmTree__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<Transform, global::Game.SDKs.PocoSDK.NodeInfo>>().Deserialize(ref reader, formatterResolver);
                        __trmTree__b__ = true;
                        break;
                    case 1:
                        __list__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::Game.SDKs.PocoSDK.NodeInfo>>().Deserialize(ref reader, formatterResolver);
                        __list__b__ = true;
                        break;
                    case 2:
                        __rQueue__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Concurrent.ConcurrentQueue<global::System.Collections.Generic.Dictionary<string, object>>>().Deserialize(ref reader, formatterResolver);
                        __rQueue__b__ = true;
                        break;
                    case 3:
                        __canvasList__ = formatterResolver.GetFormatterWithVerify<HashSet<global::Game.SDKs.PocoSDK.NodeParams>>().Deserialize(ref reader, formatterResolver);
                        __canvasList__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::CustomDumper();
            if(__trmTree__b__) ____result.trmTree = __trmTree__;
            if(__list__b__) ____result.list = __list__;
            if(__rQueue__b__) ____result.rQueue = __rQueue__;
            if(__canvasList__b__) ____result.canvasList = __canvasList__;

            return ____result;
        }
    }


    public sealed class PerfDataManagerFormatter : global::Utf8Json.IJsonFormatter<global::PerfDataManager>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public PerfDataManagerFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("isStart"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("frame"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("frameRateCur"), 2},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("uss"), 3},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("jankSmall"), 4},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("jankBig"), 5},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("allocateMemoryForGraphicsDriver"), 6},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("monoHeapSize"), 7},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("monoUsedSize"), 8},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("tempAllocateSize"), 9},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("totalAllocateMemory"), 10},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("totalReservedMemory"), 11},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("totalUnusedReservedMemory"), 12},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("isStart"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("frame"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("frameRateCur"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("uss"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("jankSmall"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("jankBig"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("allocateMemoryForGraphicsDriver"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("monoHeapSize"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("monoUsedSize"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("tempAllocateSize"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("totalAllocateMemory"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("totalReservedMemory"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("totalUnusedReservedMemory"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::PerfDataManager value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            writer.WriteBoolean(value.isStart);
            writer.WriteRaw(this.____stringByteKeys[1]);
            writer.WriteInt32(value.frame);
            writer.WriteRaw(this.____stringByteKeys[2]);
            writer.WriteInt32(value.frameRateCur);
            writer.WriteRaw(this.____stringByteKeys[3]);
            writer.WriteInt32(value.uss);
            writer.WriteRaw(this.____stringByteKeys[4]);
            writer.WriteInt32(value.jankSmall);
            writer.WriteRaw(this.____stringByteKeys[5]);
            writer.WriteInt32(value.jankBig);
            writer.WriteRaw(this.____stringByteKeys[6]);
            writer.WriteInt64(value.allocateMemoryForGraphicsDriver);
            writer.WriteRaw(this.____stringByteKeys[7]);
            writer.WriteInt64(value.monoHeapSize);
            writer.WriteRaw(this.____stringByteKeys[8]);
            writer.WriteInt64(value.monoUsedSize);
            writer.WriteRaw(this.____stringByteKeys[9]);
            writer.WriteInt64(value.tempAllocateSize);
            writer.WriteRaw(this.____stringByteKeys[10]);
            writer.WriteInt64(value.totalAllocateMemory);
            writer.WriteRaw(this.____stringByteKeys[11]);
            writer.WriteInt64(value.totalReservedMemory);
            writer.WriteRaw(this.____stringByteKeys[12]);
            writer.WriteInt64(value.totalUnusedReservedMemory);
            
            writer.WriteEndObject();
        }

        public global::PerfDataManager Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __isStart__ = default(bool);
            var __isStart__b__ = false;
            var __frame__ = default(int);
            var __frame__b__ = false;
            var __frameRateCur__ = default(int);
            var __frameRateCur__b__ = false;
            var __uss__ = default(int);
            var __uss__b__ = false;
            var __jankSmall__ = default(int);
            var __jankSmall__b__ = false;
            var __jankBig__ = default(int);
            var __jankBig__b__ = false;
            var __allocateMemoryForGraphicsDriver__ = default(long);
            var __allocateMemoryForGraphicsDriver__b__ = false;
            var __monoHeapSize__ = default(long);
            var __monoHeapSize__b__ = false;
            var __monoUsedSize__ = default(long);
            var __monoUsedSize__b__ = false;
            var __tempAllocateSize__ = default(long);
            var __tempAllocateSize__b__ = false;
            var __totalAllocateMemory__ = default(long);
            var __totalAllocateMemory__b__ = false;
            var __totalReservedMemory__ = default(long);
            var __totalReservedMemory__b__ = false;
            var __totalUnusedReservedMemory__ = default(long);
            var __totalUnusedReservedMemory__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __isStart__ = reader.ReadBoolean();
                        __isStart__b__ = true;
                        break;
                    case 1:
                        __frame__ = reader.ReadInt32();
                        __frame__b__ = true;
                        break;
                    case 2:
                        __frameRateCur__ = reader.ReadInt32();
                        __frameRateCur__b__ = true;
                        break;
                    case 3:
                        __uss__ = reader.ReadInt32();
                        __uss__b__ = true;
                        break;
                    case 4:
                        __jankSmall__ = reader.ReadInt32();
                        __jankSmall__b__ = true;
                        break;
                    case 5:
                        __jankBig__ = reader.ReadInt32();
                        __jankBig__b__ = true;
                        break;
                    case 6:
                        __allocateMemoryForGraphicsDriver__ = reader.ReadInt64();
                        __allocateMemoryForGraphicsDriver__b__ = true;
                        break;
                    case 7:
                        __monoHeapSize__ = reader.ReadInt64();
                        __monoHeapSize__b__ = true;
                        break;
                    case 8:
                        __monoUsedSize__ = reader.ReadInt64();
                        __monoUsedSize__b__ = true;
                        break;
                    case 9:
                        __tempAllocateSize__ = reader.ReadInt64();
                        __tempAllocateSize__b__ = true;
                        break;
                    case 10:
                        __totalAllocateMemory__ = reader.ReadInt64();
                        __totalAllocateMemory__b__ = true;
                        break;
                    case 11:
                        __totalReservedMemory__ = reader.ReadInt64();
                        __totalReservedMemory__b__ = true;
                        break;
                    case 12:
                        __totalUnusedReservedMemory__ = reader.ReadInt64();
                        __totalUnusedReservedMemory__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::PerfDataManager();
            if(__isStart__b__) ____result.isStart = __isStart__;
            if(__frame__b__) ____result.frame = __frame__;
            if(__frameRateCur__b__) ____result.frameRateCur = __frameRateCur__;
            if(__uss__b__) ____result.uss = __uss__;
            if(__jankSmall__b__) ____result.jankSmall = __jankSmall__;
            if(__jankBig__b__) ____result.jankBig = __jankBig__;
            if(__allocateMemoryForGraphicsDriver__b__) ____result.allocateMemoryForGraphicsDriver = __allocateMemoryForGraphicsDriver__;
            if(__monoHeapSize__b__) ____result.monoHeapSize = __monoHeapSize__;
            if(__monoUsedSize__b__) ____result.monoUsedSize = __monoUsedSize__;
            if(__tempAllocateSize__b__) ____result.tempAllocateSize = __tempAllocateSize__;
            if(__totalAllocateMemory__b__) ____result.totalAllocateMemory = __totalAllocateMemory__;
            if(__totalReservedMemory__b__) ____result.totalReservedMemory = __totalReservedMemory__;
            if(__totalUnusedReservedMemory__b__) ____result.totalUnusedReservedMemory = __totalUnusedReservedMemory__;

            return ____result;
        }
    }


    public sealed class PocoManagerFormatter : global::Utf8Json.IJsonFormatter<global::PocoManager>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public PocoManagerFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("port"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("server"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("pdm"), 2},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("port"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("server"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("pdm"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::PocoManager value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            writer.WriteInt32(value.port);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<global::TcpServer.AsyncTcpServer>().Serialize(ref writer, value.server, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<global::PerfDataManager>().Serialize(ref writer, value.pdm, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::PocoManager Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __port__ = default(int);
            var __port__b__ = false;
            var __server__ = default(global::TcpServer.AsyncTcpServer);
            var __server__b__ = false;
            var __pdm__ = default(global::PerfDataManager);
            var __pdm__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __port__ = reader.ReadInt32();
                        __port__b__ = true;
                        break;
                    case 1:
                        __server__ = formatterResolver.GetFormatterWithVerify<global::TcpServer.AsyncTcpServer>().Deserialize(ref reader, formatterResolver);
                        __server__b__ = true;
                        break;
                    case 2:
                        __pdm__ = formatterResolver.GetFormatterWithVerify<global::PerfDataManager>().Deserialize(ref reader, formatterResolver);
                        __pdm__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::PocoManager();
            if(__port__b__) ____result.port = __port__;
            if(__server__b__) ____result.server = __server__;
            if(__pdm__b__) ____result.pdm = __pdm__;

            return ____result;
        }
    }


    public sealed class DelayTaskObjFormatter : global::Utf8Json.IJsonFormatter<global::DelayTaskObj>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public DelayTaskObjFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("client"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("idAction"), 1},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("client"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("idAction"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::DelayTaskObj value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<global::TcpServer.TcpClientState>().Serialize(ref writer, value.client, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<object>().Serialize(ref writer, value.idAction, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::DelayTaskObj Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }
            

            var __client__ = default(global::TcpServer.TcpClientState);
            var __client__b__ = false;
            var __idAction__ = default(object);
            var __idAction__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __client__ = formatterResolver.GetFormatterWithVerify<global::TcpServer.TcpClientState>().Deserialize(ref reader, formatterResolver);
                        __client__b__ = true;
                        break;
                    case 1:
                        __idAction__ = formatterResolver.GetFormatterWithVerify<object>().Deserialize(ref reader, formatterResolver);
                        __idAction__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::DelayTaskObj();
            if(__client__b__) ____result.client = __client__;
            if(__idAction__b__) ____result.idAction = __idAction__;

            return ____result;
        }
    }

}

#pragma warning disable 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Utf8Json.Formatters.TcpServer
{
    using System;
    using Utf8Json;


    public sealed class TcpClientStateFormatter : global::Utf8Json.IJsonFormatter<global::TcpServer.TcpClientState>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public TcpClientStateFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("TcpClient"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Buffer"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Prot"), 2},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("NetworkStream"), 3},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("TcpClient"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Buffer"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Prot"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("NetworkStream"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::TcpServer.TcpClientState value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<TcpClient>().Serialize(ref writer, value.TcpClient, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref writer, value.Buffer, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<global::TcpServer.ProtoFilter>().Serialize(ref writer, value.Prot, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[3]);
            formatterResolver.GetFormatterWithVerify<NetworkStream>().Serialize(ref writer, value.NetworkStream, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::TcpServer.TcpClientState Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __TcpClient__ = default(TcpClient);
            var __TcpClient__b__ = false;
            var __Buffer__ = default(byte[]);
            var __Buffer__b__ = false;
            var __Prot__ = default(global::TcpServer.ProtoFilter);
            var __Prot__b__ = false;
            var __NetworkStream__ = default(NetworkStream);
            var __NetworkStream__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __TcpClient__ = formatterResolver.GetFormatterWithVerify<TcpClient>().Deserialize(ref reader, formatterResolver);
                        __TcpClient__b__ = true;
                        break;
                    case 1:
                        __Buffer__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(ref reader, formatterResolver);
                        __Buffer__b__ = true;
                        break;
                    case 2:
                        __Prot__ = formatterResolver.GetFormatterWithVerify<global::TcpServer.ProtoFilter>().Deserialize(ref reader, formatterResolver);
                        __Prot__b__ = true;
                        break;
                    case 3:
                        __NetworkStream__ = formatterResolver.GetFormatterWithVerify<NetworkStream>().Deserialize(ref reader, formatterResolver);
                        __NetworkStream__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::TcpServer.TcpClientState(__TcpClient__, __Buffer__, __Prot__);

            return ____result;
        }
    }


    public sealed class TcpClientConnectedEventArgsFormatter : global::Utf8Json.IJsonFormatter<global::TcpServer.TcpClientConnectedEventArgs>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public TcpClientConnectedEventArgsFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("TcpClient"), 0},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("TcpClient"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::TcpServer.TcpClientConnectedEventArgs value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<TcpClient>().Serialize(ref writer, value.TcpClient, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::TcpServer.TcpClientConnectedEventArgs Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __TcpClient__ = default(TcpClient);
            var __TcpClient__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __TcpClient__ = formatterResolver.GetFormatterWithVerify<TcpClient>().Deserialize(ref reader, formatterResolver);
                        __TcpClient__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::TcpServer.TcpClientConnectedEventArgs(__TcpClient__);

            return ____result;
        }
    }


    public sealed class TcpClientDisconnectedEventArgsFormatter : global::Utf8Json.IJsonFormatter<global::TcpServer.TcpClientDisconnectedEventArgs>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public TcpClientDisconnectedEventArgsFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("TcpClient"), 0},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("TcpClient"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::TcpServer.TcpClientDisconnectedEventArgs value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<TcpClient>().Serialize(ref writer, value.TcpClient, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::TcpServer.TcpClientDisconnectedEventArgs Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __TcpClient__ = default(TcpClient);
            var __TcpClient__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __TcpClient__ = formatterResolver.GetFormatterWithVerify<TcpClient>().Deserialize(ref reader, formatterResolver);
                        __TcpClient__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::TcpServer.TcpClientDisconnectedEventArgs(__TcpClient__);

            return ____result;
        }
    }

}

#pragma warning disable 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Utf8Json.Formatters.Poco
{
    using System;
    using Utf8Json;


    public sealed class AbstractNodeFormatter : global::Utf8Json.IJsonFormatter<global::Poco.AbstractNode>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public AbstractNodeFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("requiredAttrs"), 0},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("requiredAttrs"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::Poco.AbstractNode value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Serialize(ref writer, value.requiredAttrs, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::Poco.AbstractNode Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __requiredAttrs__ = default(global::System.Collections.Generic.List<string>);
            var __requiredAttrs__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __requiredAttrs__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Deserialize(ref reader, formatterResolver);
                        __requiredAttrs__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::Poco.AbstractNode();
            if(__requiredAttrs__b__) ____result.requiredAttrs = __requiredAttrs__;

            return ____result;
        }
    }


    public sealed class RootNodeFormatter : global::Utf8Json.IJsonFormatter<global::Poco.RootNode>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public RootNodeFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("children"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("payLoad"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("requiredAttrs"), 2},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("children"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("payLoad"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("requiredAttrs"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::Poco.RootNode value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::Poco.AbstractNode>>().Serialize(ref writer, value.children, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, object>>().Serialize(ref writer, value.payLoad, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Serialize(ref writer, value.requiredAttrs, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::Poco.RootNode Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __children__ = default(global::System.Collections.Generic.List<global::Poco.AbstractNode>);
            var __children__b__ = false;
            var __payLoad__ = default(global::System.Collections.Generic.Dictionary<string, object>);
            var __payLoad__b__ = false;
            var __requiredAttrs__ = default(global::System.Collections.Generic.List<string>);
            var __requiredAttrs__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __children__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::Poco.AbstractNode>>().Deserialize(ref reader, formatterResolver);
                        __children__b__ = true;
                        break;
                    case 1:
                        __payLoad__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, object>>().Deserialize(ref reader, formatterResolver);
                        __payLoad__b__ = true;
                        break;
                    case 2:
                        __requiredAttrs__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Deserialize(ref reader, formatterResolver);
                        __requiredAttrs__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::Poco.RootNode();
            if(__children__b__) ____result.children = __children__;
            if(__payLoad__b__) ____result.payLoad = __payLoad__;
            if(__requiredAttrs__b__) ____result.requiredAttrs = __requiredAttrs__;

            return ____result;
        }
    }


    public sealed class UnityNodeFormatter : global::Utf8Json.IJsonFormatter<global::Poco.UnityNode>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;
    
        public UnityNodeFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("children"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("payLoad"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("requiredAttrs"), 2},
            };
    
            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("children"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("payLoad"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("requiredAttrs"),
                
            };
        }
    
        public void Serialize(ref JsonWriter writer, global::Poco.UnityNode value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            
        
            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<Transform>>().Serialize(ref writer, value.children, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, object>>().Serialize(ref writer, value.payLoad, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Serialize(ref writer, value.requiredAttrs, formatterResolver);
            
            writer.WriteEndObject();
        }
    
        public global::Poco.UnityNode Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            
    
            var __children__ = default(global::System.Collections.Generic.List<Transform>);
            var __children__b__ = false;
            var __payLoad__ = default(global::System.Collections.Generic.Dictionary<string, object>);
            var __payLoad__b__ = false;
            var __requiredAttrs__ = default(global::System.Collections.Generic.List<string>);
            var __requiredAttrs__b__ = false;
    
            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }
    
                switch (key)
                {
                    case 0:
                        __children__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<Transform>>().Deserialize(ref reader, formatterResolver);
                        __children__b__ = true;
                        break;
                    case 1:
                        __payLoad__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, object>>().Deserialize(ref reader, formatterResolver);
                        __payLoad__b__ = true;
                        break;
                    case 2:
                        __requiredAttrs__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Deserialize(ref reader, formatterResolver);
                        __requiredAttrs__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }
    
                NEXT_LOOP:
                continue;
            }
    
            var ____result = new global::Poco.UnityNode();
            if(__children__b__) ____result.children = __children__;
            if(__payLoad__b__) ____result.payLoad = __payLoad__;
            if(__requiredAttrs__b__) ____result.requiredAttrs = __requiredAttrs__;
    
            return ____result;
        }
    }

}

#pragma warning disable 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Utf8Json.Formatters.Utf8Json
{
    using System;
    using Utf8Json;


    public sealed class JsonFormatterAttributeFormatter : global::Utf8Json.IJsonFormatter<global::Utf8Json.JsonFormatterAttribute>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public JsonFormatterAttributeFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("FormatterType"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Arguments"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("TypeId"), 2},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("FormatterType"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Arguments"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("TypeId"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::Utf8Json.JsonFormatterAttribute value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<global::System.Type>().Serialize(ref writer, value.FormatterType, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<object[]>().Serialize(ref writer, value.Arguments, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<object>().Serialize(ref writer, value.TypeId, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::Utf8Json.JsonFormatterAttribute Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __FormatterType__ = default(global::System.Type);
            var __FormatterType__b__ = false;
            var __Arguments__ = default(object[]);
            var __Arguments__b__ = false;
            var __TypeId__ = default(object);
            var __TypeId__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __FormatterType__ = formatterResolver.GetFormatterWithVerify<global::System.Type>().Deserialize(ref reader, formatterResolver);
                        __FormatterType__b__ = true;
                        break;
                    case 1:
                        __Arguments__ = formatterResolver.GetFormatterWithVerify<object[]>().Deserialize(ref reader, formatterResolver);
                        __Arguments__b__ = true;
                        break;
                    case 2:
                        __TypeId__ = formatterResolver.GetFormatterWithVerify<object>().Deserialize(ref reader, formatterResolver);
                        __TypeId__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::Utf8Json.JsonFormatterAttribute(__FormatterType__);

            return ____result;
        }
    }


    public sealed class SerializationConstructorAttributeFormatter : global::Utf8Json.IJsonFormatter<global::Utf8Json.SerializationConstructorAttribute>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public SerializationConstructorAttributeFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("TypeId"), 0},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("TypeId"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::Utf8Json.SerializationConstructorAttribute value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<object>().Serialize(ref writer, value.TypeId, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::Utf8Json.SerializationConstructorAttribute Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __TypeId__ = default(object);
            var __TypeId__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __TypeId__ = formatterResolver.GetFormatterWithVerify<object>().Deserialize(ref reader, formatterResolver);
                        __TypeId__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::Utf8Json.SerializationConstructorAttribute();

            return ____result;
        }
    }


    public sealed class FormatterNotRegisteredExceptionFormatter : global::Utf8Json.IJsonFormatter<global::Utf8Json.FormatterNotRegisteredException>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public FormatterNotRegisteredExceptionFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Message"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Data"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("InnerException"), 2},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("TargetSite"), 3},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("StackTrace"), 4},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("HelpLink"), 5},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Source"), 6},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("HResult"), 7},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Message"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Data"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("InnerException"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("TargetSite"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("StackTrace"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("HelpLink"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Source"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("HResult"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::Utf8Json.FormatterNotRegisteredException value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            writer.WriteString(value.Message);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.IDictionary>().Serialize(ref writer, value.Data, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<global::System.Exception>().Serialize(ref writer, value.InnerException, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[3]);
            formatterResolver.GetFormatterWithVerify<global::System.Reflection.MethodBase>().Serialize(ref writer, value.TargetSite, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[4]);
            writer.WriteString(value.StackTrace);
            writer.WriteRaw(this.____stringByteKeys[5]);
            writer.WriteString(value.HelpLink);
            writer.WriteRaw(this.____stringByteKeys[6]);
            writer.WriteString(value.Source);
            writer.WriteRaw(this.____stringByteKeys[7]);
            writer.WriteInt32(value.HResult);
            
            writer.WriteEndObject();
        }

        public global::Utf8Json.FormatterNotRegisteredException Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __Message__ = default(string);
            var __Message__b__ = false;
            var __Data__ = default(global::System.Collections.IDictionary);
            var __Data__b__ = false;
            var __InnerException__ = default(global::System.Exception);
            var __InnerException__b__ = false;
            var __TargetSite__ = default(global::System.Reflection.MethodBase);
            var __TargetSite__b__ = false;
            var __StackTrace__ = default(string);
            var __StackTrace__b__ = false;
            var __HelpLink__ = default(string);
            var __HelpLink__b__ = false;
            var __Source__ = default(string);
            var __Source__b__ = false;
            var __HResult__ = default(int);
            var __HResult__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Message__ = reader.ReadString();
                        __Message__b__ = true;
                        break;
                    case 1:
                        __Data__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.IDictionary>().Deserialize(ref reader, formatterResolver);
                        __Data__b__ = true;
                        break;
                    case 2:
                        __InnerException__ = formatterResolver.GetFormatterWithVerify<global::System.Exception>().Deserialize(ref reader, formatterResolver);
                        __InnerException__b__ = true;
                        break;
                    case 3:
                        __TargetSite__ = formatterResolver.GetFormatterWithVerify<global::System.Reflection.MethodBase>().Deserialize(ref reader, formatterResolver);
                        __TargetSite__b__ = true;
                        break;
                    case 4:
                        __StackTrace__ = reader.ReadString();
                        __StackTrace__b__ = true;
                        break;
                    case 5:
                        __HelpLink__ = reader.ReadString();
                        __HelpLink__b__ = true;
                        break;
                    case 6:
                        __Source__ = reader.ReadString();
                        __Source__b__ = true;
                        break;
                    case 7:
                        __HResult__ = reader.ReadInt32();
                        __HResult__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::Utf8Json.FormatterNotRegisteredException(__Message__);
            if(__HelpLink__b__) ____result.HelpLink = __HelpLink__;
            if(__Source__b__) ____result.Source = __Source__;

            return ____result;
        }
    }


    public sealed class JsonParsingExceptionFormatter : global::Utf8Json.IJsonFormatter<global::Utf8Json.JsonParsingException>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public JsonParsingExceptionFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Offset"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("ActualChar"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Message"), 2},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Data"), 3},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("InnerException"), 4},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("TargetSite"), 5},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("StackTrace"), 6},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("HelpLink"), 7},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Source"), 8},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("HResult"), 9},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Offset"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("ActualChar"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Message"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Data"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("InnerException"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("TargetSite"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("StackTrace"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("HelpLink"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Source"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("HResult"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::Utf8Json.JsonParsingException value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            writer.WriteInt32(value.Offset);
            writer.WriteRaw(this.____stringByteKeys[1]);
            writer.WriteString(value.ActualChar);
            writer.WriteRaw(this.____stringByteKeys[2]);
            writer.WriteString(value.Message);
            writer.WriteRaw(this.____stringByteKeys[3]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.IDictionary>().Serialize(ref writer, value.Data, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[4]);
            formatterResolver.GetFormatterWithVerify<global::System.Exception>().Serialize(ref writer, value.InnerException, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[5]);
            formatterResolver.GetFormatterWithVerify<global::System.Reflection.MethodBase>().Serialize(ref writer, value.TargetSite, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[6]);
            writer.WriteString(value.StackTrace);
            writer.WriteRaw(this.____stringByteKeys[7]);
            writer.WriteString(value.HelpLink);
            writer.WriteRaw(this.____stringByteKeys[8]);
            writer.WriteString(value.Source);
            writer.WriteRaw(this.____stringByteKeys[9]);
            writer.WriteInt32(value.HResult);
            
            writer.WriteEndObject();
        }

        public global::Utf8Json.JsonParsingException Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __Offset__ = default(int);
            var __Offset__b__ = false;
            var __ActualChar__ = default(string);
            var __ActualChar__b__ = false;
            var __Message__ = default(string);
            var __Message__b__ = false;
            var __Data__ = default(global::System.Collections.IDictionary);
            var __Data__b__ = false;
            var __InnerException__ = default(global::System.Exception);
            var __InnerException__b__ = false;
            var __TargetSite__ = default(global::System.Reflection.MethodBase);
            var __TargetSite__b__ = false;
            var __StackTrace__ = default(string);
            var __StackTrace__b__ = false;
            var __HelpLink__ = default(string);
            var __HelpLink__b__ = false;
            var __Source__ = default(string);
            var __Source__b__ = false;
            var __HResult__ = default(int);
            var __HResult__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Offset__ = reader.ReadInt32();
                        __Offset__b__ = true;
                        break;
                    case 1:
                        __ActualChar__ = reader.ReadString();
                        __ActualChar__b__ = true;
                        break;
                    case 2:
                        __Message__ = reader.ReadString();
                        __Message__b__ = true;
                        break;
                    case 3:
                        __Data__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.IDictionary>().Deserialize(ref reader, formatterResolver);
                        __Data__b__ = true;
                        break;
                    case 4:
                        __InnerException__ = formatterResolver.GetFormatterWithVerify<global::System.Exception>().Deserialize(ref reader, formatterResolver);
                        __InnerException__b__ = true;
                        break;
                    case 5:
                        __TargetSite__ = formatterResolver.GetFormatterWithVerify<global::System.Reflection.MethodBase>().Deserialize(ref reader, formatterResolver);
                        __TargetSite__b__ = true;
                        break;
                    case 6:
                        __StackTrace__ = reader.ReadString();
                        __StackTrace__b__ = true;
                        break;
                    case 7:
                        __HelpLink__ = reader.ReadString();
                        __HelpLink__b__ = true;
                        break;
                    case 8:
                        __Source__ = reader.ReadString();
                        __Source__b__ = true;
                        break;
                    case 9:
                        __HResult__ = reader.ReadInt32();
                        __HResult__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::Utf8Json.JsonParsingException(__Message__);
            if(__ActualChar__b__) ____result.ActualChar = __ActualChar__;
            if(__HelpLink__b__) ____result.HelpLink = __HelpLink__;
            if(__Source__b__) ____result.Source = __Source__;

            return ____result;
        }
    }

}

#pragma warning disable 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Utf8Json.Formatters.Utf8Json.Resolvers
{
    using System;
    using Utf8Json;


    public sealed class DynamicCompositeResolverFormatter : global::Utf8Json.IJsonFormatter<global::Utf8Json.Resolvers.DynamicCompositeResolver>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public DynamicCompositeResolverFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("formatters"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("resolvers"), 1},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("formatters"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("resolvers"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::Utf8Json.Resolvers.DynamicCompositeResolver value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<global::Utf8Json.IJsonFormatter[]>().Serialize(ref writer, value.formatters, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<global::Utf8Json.IJsonFormatterResolver[]>().Serialize(ref writer, value.resolvers, formatterResolver);
            
            writer.WriteEndObject();
        }

        
        public global::Utf8Json.Resolvers.DynamicCompositeResolver Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __formatters__ = default(global::Utf8Json.IJsonFormatter[]);
            var __formatters__b__ = false;
            var __resolvers__ = default(global::Utf8Json.IJsonFormatterResolver[]);
            var __resolvers__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __formatters__ = formatterResolver.GetFormatterWithVerify<global::Utf8Json.IJsonFormatter[]>().Deserialize(ref reader, formatterResolver);
                        __formatters__b__ = true;
                        break;
                    case 1:
                        __resolvers__ = formatterResolver.GetFormatterWithVerify<global::Utf8Json.IJsonFormatterResolver[]>().Deserialize(ref reader, formatterResolver);
                        __resolvers__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new DynamicCompositeResolverImpl(__formatters__, __resolvers__);

            return ____result;
            return null;//____result;
        }
        
    }

    public class DynamicCompositeResolverImpl : global::Utf8Json.Resolvers.DynamicCompositeResolver
    {
        public DynamicCompositeResolverImpl(IJsonFormatter[] formatters, IJsonFormatterResolver[] resolvers):base(formatters, resolvers)
        {
            
        }

        public override IJsonFormatter<T> GetFormatter<T>()
        {
            return null;
        }
        
    }

}

#pragma warning disable 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
