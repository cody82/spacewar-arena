/*
 * Copyright 2006 Sony Computer Entertainment Inc.
 * 
 * Licensed under the SCEA Shared Source License, Version 1.0 (the "License"); you may not use this 
 * file except in compliance with the License. You may obtain a copy of the License at:
 * http://research.scea.com/scea_shared_source_license.html
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License 
 * is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
 * implied. See the License for the specific language governing permissions and limitations under the 
 * License.
 */

#region Using Statements
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using COLLADA;
#endregion

namespace COLLADA
{

    public class Util
    {
        /// <summary>
        /// Helper function, returns the p[] value for the given index
        /// </summary>
        /// <param name="input">The "<input>" element we need the index of.</param>
        /// <param name="primitive">The "<primitive>" element the "<input>" is from.</param>
        /// <param name="index"> The index for which we need the p[] value.</param>
        static public int GetPValue(Document.Input input, Document.Primitive primitive, int index)
        {
            int stride = primitive.stride;
            int offset = input.offset;
            return primitive.p[index * stride + offset];
        }
        /// <summary>
        /// Helper function, returns the element of a source for the given index
        /// </summary>
        /// <param name="doc">The COLLADA document</param>
        /// <param name="input">The "<input>" element we need the index of.</param>
        /// <param name="index"> The index for which we need the value.</param>
        public static float[] GetSourceElement(Document doc, Document.Input input, int index)
        {
            Document.Source src = (Document.Source)input.source;
            int stride = src.accessor.stride;
            int offset = src.accessor.offset;

            // resolve array
            // Note: this will work only if the array is in the current document...
            //       TODO: create a resolver funtion rather than access to doc.dic directly...
            //             enable loading array from binary raw file as well

            object array = doc.dic[src.accessor.source.Fragment];

            if (array is Document.Array<float>)
            {
                Document.Array<float> farray = (Document.Array<float>)(array);

                if (src.accessor.ParameterCount == 1)
                {
                    float[] returnValue = new float[1];
                    returnValue[0] = farray[src.accessor[0, index]];
                    return returnValue;
                }
                else if (src.accessor.ParameterCount == 2)
                {
                    float[] returnValue = new float[2];
                    returnValue[0] = farray[src.accessor[0, index]];
                    returnValue[1] = farray[offset + index * stride + src.accessor[1]];
                    return returnValue;
                }
                else if (src.accessor.ParameterCount == 3)
                {
                    float[] returnValue = new float[3];
                    returnValue[0] = farray[src.accessor[0, index]];
                    returnValue[1] = farray[offset + index * stride + src.accessor[1]];
                    returnValue[2] = farray[offset + index * stride + src.accessor[2]];
                    return returnValue;
                }
                else throw new Exception("Unsupported accessor size");
            }
            else
                throw new Exception("Unsupported array type");
            // Note: Rarelly int_array could be used for geometry values
        }
        /// <summary>
        /// Helper function, returns the "<input>" that has the POSITION semantic
        /// </summary>
        /// <param name="mesh">The "<mesh>" element we need the POSITION from.</param>
        static public Document.Input GetPositionInput(Document.Mesh mesh)
        {
            int i;
            for (i = 0; i < mesh.vertices.inputs.Count; i++)
            {
                if (mesh.vertices.inputs[i].semantic == "POSITION")
                    return mesh.vertices.inputs[i];
            }
            throw new Exception("No POSITION in vertex input");
        }
        /// <summary>
        /// Helper function. Returns all the inputs of the primitive. 
        /// Resolve the 'VERTEX' indirection case.
        /// <param name="doc">The COLLADA document</param>
        /// <param name="primitive"> The "<primitive>" we need the inputs from.</param>
        /// </summary>
        static public List<Document.Input> getAllInputs(Document doc, Document.Primitive primitive)
        {
            List<Document.Input> inputs = new List<Document.Input>();
            Document.Input vertexInput = null;

            // 1- get all the regular inputs
            foreach (Document.Input input in primitive.Inputs)
            {
                if (input.semantic == "VERTEX")
                    vertexInput = input;
                else
                    inputs.Add(new Document.Input(doc, input.offset, input.semantic, input.set, ((Document.Source)input.source).id));
            }
            // 2- get all the indirect inputs
            if (vertexInput != null)
                foreach (Document.Input input in ((Document.Vertices)vertexInput.source).inputs)
                {
                    inputs.Add(new Document.Input(doc, vertexInput.offset, input.semantic, input.set, ((Document.Source)input.source).id));
                }
            return inputs;
        }

    }
}