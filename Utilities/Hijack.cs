using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SpaceCore.Utilities
{
    public class Hijack
    {
        public static void hijack( MethodInfo target, MethodInfo replaceWith )
        {
            //*
            bool withThis = !target.IsStatic && replaceWith.IsStatic; // target.CallingConvention.HasFlag(CallingConventions.HasThis);
            if (target.ReturnType != replaceWith.ReturnType)
                throw new ArgumentException("Target and replacement methods must match; return type");
            if (target.GetParameters().Length != replaceWith.GetParameters().Length - (withThis ? 1 : 0))
                throw new ArgumentException("Target and replacement methods must match; parameter count");
            if ( withThis && target.DeclaringType != replaceWith.GetParameters()[ 0 ].ParameterType)
                throw new ArgumentException("Target and replacement methods must match; parameter 0 (this)");
            for (int i = 0; i < target.GetParameters().Length; ++i)
            {
                var tparam = target.GetParameters()[i];
                var rparam = replaceWith.GetParameters()[i + (withThis ? 1 : 0)];
                if (tparam.ParameterType != rparam.ParameterType)
                    throw new ArgumentException("Target and replacement methods must match; parameter " + (i + + (withThis ? 1 : 0)));
            }
            //*/
            Log.debug("Hijacking method \"" + target + "\", replacing with \"" + replaceWith + "\".");

            RuntimeHelpers.PrepareMethod(target.MethodHandle);
            RuntimeHelpers.PrepareMethod(replaceWith.MethodHandle);

            if (target.IsVirtual)
            {/*
                // http://stackoverflow.com/a/38783635
                unsafe
                {
                    UI
                }*/
            }
            //else
            {
                    // http://stackoverflow.com/a/36415711
                    unsafe
                    {
                    int insOffset = 1;
                    if (IntPtr.Size == 4)
                    {
                        insOffset = 2;
                    }
                    Log.trace("Offset: " + insOffset);

                    int* ttar = (int*)target.MethodHandle.Value.ToPointer() + insOffset;
                    int* rtar = (int*)replaceWith.MethodHandle.Value.ToPointer() + insOffset;

                    // Debugger.IsAttached section not needed with VS2017? Or whatever caused the change
                    if (false&&Debugger.IsAttached)
                    {
                        Log.trace("Debugger is attached.");

                        byte* tinsn = (byte*)(*ttar);
                        byte* rinsn = (byte*)(*rtar);

                        int* tsrc = (int*)(tinsn + 1);
                        int* rsrc = (int*)(rinsn + 1);

                        Log.trace("Data (1): " + new IntPtr(ttar) + "=" + (ttar == null ? 0 : (*ttar)) + " " + new IntPtr(rtar) + "=" + (rtar == null ? 0 : (*rtar)));
                        Log.trace("Data (2): " + new IntPtr(tinsn) + "=" + (tinsn == null ? 0 : (*tinsn)) + " " + new IntPtr(rinsn) + "=" + (rinsn == null ? 0 : (*rinsn)));
                        Log.trace("Data (3): " + new IntPtr(tsrc) + "=" + (tinsn == null ? 1 : (*tsrc)) + " " + new IntPtr(rsrc) + "=" + (rinsn == null ? 1 : (*rsrc)));
                        (*tsrc) = (((int)rinsn + 5) + (*rsrc)) - ((int)tinsn + 5);
                    }
                    else
                    {
                        Log.trace("Debugger is not attached.");
                        Log.trace("Data: " + new IntPtr(ttar) + "=" + (*ttar) + " " + new IntPtr(rtar) + "=" + (*rtar));
                        (*ttar) = (*rtar);
                    }
                }
            }

            Log.trace("Done");
        }
    }
}
