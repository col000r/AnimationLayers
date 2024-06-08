// Copyright (c) 2020 Markus Hofer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using FaderX;
using UnityEngine;

public class LerpDriver : MonoBehaviour {

    public static LerpDriver Instance;
    
    public static void LerpTo() {
        Init();
        
    }

    public static void Init() {
        if( Instance == null ) {
            GameObject go = new GameObject( "LerpDriver" );
            Instance = go.AddComponent<LerpDriver>();
        }
    }


    [SerializeField] private List<FloatLerper> m_FloatLerpers = new List<FloatLerper>();

    private void Update() {
        foreach( FloatLerper lerper in m_FloatLerpers ) {
            if( !lerper.Done ) {
                //TODO: Figure out callback, etc.
            }
        }
    }
}

public static class LerpExtensions {
    //public static void 
}
