/*
 * Copyright (C) 2007-2010 Júlio Vilmar Gesser.
 * Copyright (C) 2011, 2013-2020 The JavaParser Team.
 *
 * This file is part of JavaParser.
 *
 * JavaParser can be used either under the terms of
 * a) the GNU Lesser General Public License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 * b) the terms of the Apache License
 *
 * You should have received a copy of both licenses in LICENCE.LGPL and
 * LICENCE.APACHE. Please refer to those files for details.
 *
 * JavaParser is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 */


import com.github.javaparser.ast.Node;
import com.github.javaparser.ast.NodeList;
import com.github.javaparser.ast.expr.FieldAccessExpr;
import com.github.javaparser.ast.expr.MethodCallExpr;
import com.github.javaparser.ast.expr.NameExpr;
import com.github.javaparser.ast.expr.SimpleName;
import com.github.javaparser.ast.nodeTypes.NodeWithOptionalScope;
import com.github.javaparser.ast.nodeTypes.NodeWithScope;
import com.github.javaparser.metamodel.NodeMetaModel;
import com.github.javaparser.metamodel.PropertyMetaModel;
import com.github.javaparser.resolution.declarations.ResolvedFieldDeclaration;
import com.github.javaparser.resolution.declarations.ResolvedReferenceTypeDeclaration;
import com.github.javaparser.resolution.declarations.ResolvedValueDeclaration;
import com.github.javaparser.resolution.types.ResolvedType;
import com.github.javaparser.symbolsolver.JavaSymbolSolver;
import com.github.javaparser.symbolsolver.model.typesystem.ReferenceTypeImpl;

import java.util.List;
import java.util.Optional;

import static com.github.javaparser.utils.Utils.assertNotNull;
import static java.util.stream.Collectors.toList;

/**
 * Outputs an XML file containing the AST meant for inspecting it.
 * Modified by MHK to fix bugs 2749 and 2750
 */
public class XmlPrinterMHK {

    private JavaSymbolSolver solver;
    private final boolean outputNodeType;

    public XmlPrinterMHK(JavaSymbolSolver solver, boolean outputNodeType) {
        this.outputNodeType = outputNodeType;
        this.solver = solver;
    }

    public String output(Node node) {
        StringBuilder output = new StringBuilder();
        output(node, "root", 0, output);
        return output.toString();
    }

    public void output(Node node, String name, int level, StringBuilder builder) {
        assertNotNull(node);
        NodeMetaModel metaModel = node.getMetaModel();
        List<PropertyMetaModel> allPropertyMetaModels = metaModel.getAllPropertyMetaModels();
        List<PropertyMetaModel> attributes = allPropertyMetaModels.stream().filter(PropertyMetaModel::isAttribute).filter(PropertyMetaModel::isSingular).collect(toList());
        List<PropertyMetaModel> subNodes = allPropertyMetaModels.stream().filter(PropertyMetaModel::isNode).filter(PropertyMetaModel::isSingular).collect(toList());
        List<PropertyMetaModel> subLists = allPropertyMetaModels.stream().filter(PropertyMetaModel::isNodeList).collect(toList());

        builder.append("<").append(name);
        if (outputNodeType) {
            builder.append(attribute("nodeType", metaModel.getTypeName()));
        }

        for (PropertyMetaModel attributeMetaModel : attributes) {
            builder.append(attribute(attributeMetaModel.getName(), attributeMetaModel.getValue(node).toString()));
        }
        
        if (node instanceof MethodCallExpr) {
            try {
                SimpleName methodName = ((MethodCallExpr) node).getName();
                String returnType = ((MethodCallExpr) node).resolve().getReturnType().describe();
                builder.append(attribute("RETURN", returnType));
            } catch (Exception e) {
                // no action
            }
        }
        if (node instanceof NameExpr) {
            try {
                ResolvedValueDeclaration decl = ((NameExpr) node).resolve();
                // Detect a reference to a value in an enumeration class
                if (decl.isField() && decl.asField().declaringType().isEnum()) {
                    builder.append(attribute("ENUM_TYPE", decl.asField().declaringType().getQualifiedName()));
                }
                ResolvedType resolvedType = ((NameExpr) node).calculateResolvedType();
                builder.append(attribute("RESOLVED_TYPE", resolvedType.describe()));
            } catch (Exception e) {
                builder.append(attribute("UNRESOLVED", node.toString()));
            }
        } else if (node instanceof FieldAccessExpr) {
            // try to determine if we are accessing a static field of a generic type (notably Feature<T>)
            try {
                String returnType = ((FieldAccessExpr) node).resolve().getType().describe();
                builder.append(attribute("RETURN", returnType));
            } catch (Exception e) {
                // no action
            }
            try {

                ResolvedType resolvedType = ((NodeWithScope<?>) node).getScope().calculateResolvedType();
                builder.append(attribute("RESOLVED_TYPE", resolvedType.describe()));
                if (resolvedType instanceof ReferenceTypeImpl) {
                    Optional<ResolvedReferenceTypeDeclaration> decl = ((ReferenceTypeImpl) resolvedType).getTypeDeclaration();
                    if (decl.isPresent() && !decl.get().getTypeParameters().isEmpty()) {
                        ResolvedFieldDeclaration fieldDecl = decl.get().getField(((FieldAccessExpr) node).getNameAsString());
                        if (fieldDecl.isStatic()) {
                            ResolvedType fieldType = fieldDecl.asField().getType();
                            fieldType.describe();
                            builder.append(attribute("FIELD_TYPE", fieldType.describe()));
                        }
                    }
                }
            } catch (Exception e) {
                builder.append(attribute("UNRESOLVED", ((NodeWithScope<?>) node).getScope().toString()));
            }
        } else if (node instanceof NodeWithScope) {
            try {
                ResolvedType resolvedType = ((NodeWithScope<?>) node).getScope().calculateResolvedType();
                //System.err.println("Resolved symbol: " + ((NodeWithScope<?>) node).getScope().toString() + " => " + resolvedType.describe());
                builder.append(attribute("RESOLVED_TYPE", resolvedType.describe()));
            } catch (RuntimeException e) {
                builder.append(attribute("UNRESOLVED", ((NodeWithScope<?>) node).getScope().toString()));
            }
        } else if (node instanceof NodeWithOptionalScope) {
            try {
                Optional<com.github.javaparser.ast.expr.Expression> optScope = ((NodeWithOptionalScope) node).getScope();
                if (optScope.isPresent()) {
                    ResolvedType resolvedType = optScope.get().calculateResolvedType();
                    //System.err.println("Resolved symbol: " + ((NodeWithScope<?>) node).getScope().toString() + " => " + resolvedType.describe());
                    builder.append(attribute("RESOLVED_TYPE", resolvedType.describe()));
                } else if (node instanceof MethodCallExpr) {
                    String qualifiedMethodName = ((MethodCallExpr) node).resolve().getQualifiedName();
                    int lastDot = qualifiedMethodName.lastIndexOf(".");
                    builder.append(attribute("DECLARING_TYPE", qualifiedMethodName.substring(0, lastDot)));
                }
            } catch (RuntimeException e) {
                builder.append(attribute("UNRESOLVED", ((NodeWithOptionalScope<?>) node).getScope().toString()));
            }
        } else if (node instanceof com.github.javaparser.ast.type.Type) {
            try {
                ResolvedType resolvedType = ((com.github.javaparser.ast.type.Type) node).resolve();
                builder.append(attribute("RESOLVED_TYPE", resolvedType.describe()));
            } catch (RuntimeException e) {
                //System.err.println("Unresolved: " + node.toString() + " at " + node.getRange());
                builder.append(attribute("UNRESOLVED", node.toString()));
            }
        }

        builder.append(">");

        for (PropertyMetaModel subNodeMetaModel : subNodes) {
            Node value = (Node) subNodeMetaModel.getValue(node);
            if (value != null) {
                output(value, subNodeMetaModel.getName(), level + 1, builder);
            }
        }

        for (PropertyMetaModel subListMetaModel : subLists) {
            NodeList<? extends Node> subList = (NodeList<? extends Node>) subListMetaModel.getValue(node);
            if (subList != null) {
                String listName = subListMetaModel.getName();
                if (subList.isEmpty()) {
                    if (listName.equals("typeArguments")) {
                        // MHK: this is the only case we know of where an empty list differs from an absent list, e.g. new HashSet<>().
                        builder.append("<").append(listName).append("/>");
                    }
                } else {
                    builder.append("<").append(listName).append(">");
                    String singular = listName.substring(0, listName.length() - 1);
                    for (Node subListNode : subList) {
                        output(subListNode, singular, level + 1, builder);
                    }
                    builder.append(close(listName));
                }
            }
        }
        builder.append(close(name));
    }

    private static String close(String name) {
        return "</" + name + ">";
    }

    private static String escape(String value) {
        return value.replace("&", "&amp;").replace("<", "&lt;").replace("'", "&apos;");
    }

    private static String attribute(String name, String value) {
        return " " + name + "='" + escape(value) + "'";
    }

//    public static void print(Node node) {
//        System.out.println(new XmlPrinterMHK(true).output(node));
//    }
}

