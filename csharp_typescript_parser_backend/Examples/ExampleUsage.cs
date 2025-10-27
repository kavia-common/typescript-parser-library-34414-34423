using System;
using TypescriptParser;

namespace Examples
{
    /// <summary>
    /// Simple usage example for the TypeScript parser library.
    /// </summary>
    internal static class ExampleUsage
    {
        public static void Run()
        {
            var source = @"
import React, { useState as useS } from 'react';
import type { FC } from 'react';

export class Counter extends BaseComponent implements IRenderable, IDisposable {
  public static version: string = '1.0.0';
  private count?: number = 0;
  readonly label: string;

  constructor(label: string) {
    this.label = label;
  }

  public increment(step: number = 1): void {
    this.count = (this.count ?? 0) + step;
  }

  render(): string {
    return `${this.label}: ${this.count}`;
  }
}
";

            ITypescriptParser parser = new TypescriptParserFacade();

            // Parse full model
            var model = parser.Parse(source);

            Console.WriteLine("Class: " + model.ClassName);
            Console.WriteLine("Extends: " + (model.BaseClass ?? "<none>"));
            Console.WriteLine("Implements: " + string.Join(", ", model.ImplementedInterfaces));
            Console.WriteLine("Imports: " + model.Imports.Count);
            Console.WriteLine("Variables:");
            foreach (var v in model.Variables)
            {
                Console.WriteLine("  - " + v.ToString());
            }
            Console.WriteLine("Functions:");
            foreach (var f in model.Functions)
            {
                Console.WriteLine("  - " + f.Name + "(" + string.Join(", ", f.Parameters) + ")");
            }

            // Extract variables quickly
            var vars = parser.Variables(source);
            Console.WriteLine("Variable count: " + vars.Count);

            // Serialize back to source
            var regenerated = parser.ToString(model);
            Console.WriteLine("-- Regenerated TS --");
            Console.WriteLine(regenerated);
        }
    }
}
